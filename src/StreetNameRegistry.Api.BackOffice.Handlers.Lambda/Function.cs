using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda
{
    using System.Reflection;
    using Abstractions;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Consumer;
    using Handlers;
    using Infrastructure;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Requests;
    using StreetNameRegistry.Infrastructure;
    using StreetNameRegistry.Infrastructure.Modules;
    using TicketingService.Proxy.HttpProxy;

    public sealed class Function : FunctionBase
    {
        protected override IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var builder = new ContainerBuilder();

            var tempProvider = services.BuildServiceProvider();
            var loggerFactory = tempProvider.GetRequiredService<ILoggerFactory>();

            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            // request & notification handlers
            builder.Register<ServiceFactory>(context =>
            {
                var ctx = context.Resolve<IComponentContext>();
                return type => ctx.Resolve(type);
            });

            builder.RegisterAssemblyTypes(typeof(MessageHandler).GetTypeInfo().Assembly).AsImplementedInterfaces();

            builder.Register(c => configuration)
                .AsSelf()
                .As<IConfiguration>()
                .SingleInstance();

            services.AddHttpProxyTicketing(configuration.GetSection("TicketingService")["InternalBaseUrl"]);

            // RETRY POLICY
            var maxRetryCount = int.Parse(configuration.GetSection("RetryPolicy")["MaxRetryCount"]);
            var startingDelaySeconds = int.Parse(configuration.GetSection("RetryPolicy")["StartingRetryDelaySeconds"]);

            builder.Register(_ => new LambdaHandlerRetryPolicy(maxRetryCount, startingDelaySeconds))
                .As<ICustomRetryPolicy>()
                .AsSelf()
                .SingleInstance();

            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

            JsonConvert.DefaultSettings = () => eventSerializerSettings;

            builder
                .RegisterModule(new DataDogModule(configuration))
                .RegisterModule<EnvelopeModule>()
                .RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings))
                .RegisterModule(new CommandHandlingModule(configuration))
                .RegisterModule(new SequenceModule(configuration, services, loggerFactory))
                .RegisterModule(new BackOfficeModule(configuration, services, loggerFactory))
                .RegisterModule(new ConsumerModule(configuration, services, loggerFactory));

            builder.RegisterModule(new IdempotencyModule(
                services,
                configuration.GetSection(IdempotencyConfiguration.Section).Get<IdempotencyConfiguration>()
                    .ConnectionString,
                new IdempotencyMigrationsTableInfo(Schema.Import),
                new IdempotencyTableInfo(Schema.Import),
                loggerFactory));

            builder.RegisterEventstreamModule(configuration);
            builder.RegisterSnapshotModule(configuration);

            builder.RegisterType<SqsStreetNameProposeLambdaHandler>()
                .As<IRequestHandler<SqsLambdaStreetNameProposeRequest>>()
                .AsSelf();

            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
    }
}