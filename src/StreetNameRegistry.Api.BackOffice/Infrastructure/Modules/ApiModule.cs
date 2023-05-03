namespace StreetNameRegistry.Api.BackOffice.Infrastructure.Modules
{
    using System;
    using System.Collections.Generic;
    using Abstractions;
    using Abstractions.SqsRequests;
    using Authorization;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Auth;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Microsoft;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance.AcmIdm;
    using Consumer.Infrastructure.Modules;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Municipality;
    using NisCodeService.DynamoDb.Extensions;
    using NisCodeService.HardCoded.Extensions;
    using StreetNameRegistry.Infrastructure;
    using StreetNameRegistry.Infrastructure.Modules;

    public sealed class ApiModule : Module
    {
        internal const string SqsQueueUrlConfigKey = "SqsQueueUrl";

        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IWebHostEnvironment _environment;

        public ApiModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _services = services;
            _loggerFactory = loggerFactory;
            _environment = environment;
        }

        protected override void Load(ContainerBuilder builder)
        {
            _services.RegisterModule(new DataDogModule(_configuration));

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder
                .RegisterType<IfMatchHeaderValidator>()
                .As<IIfMatchHeaderValidator>()
                .AsSelf();

            builder.Register(c => new AcmIdmProvenanceFactory(Application.StreetNameRegistry, c.Resolve<IActionContextAccessor>()))
                .As<IProvenanceFactory>()
                .InstancePerLifetimeScope()
                .AsSelf();

            builder
                .RegisterType<ProposeStreetNameRequestFactory>()
                .AsSelf();

            builder
                .RegisterModule(new BackOfficeModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new AggregateSourceModule(_configuration))
                .RegisterModule(new SequenceModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new MediatRModule())
                .RegisterModule(new SqsHandlersModule(_configuration[SqsQueueUrlConfigKey]))
                .RegisterModule(new TicketingModule(_configuration, _services))
                .RegisterModule(new ConsumerModule(_configuration, _services, _loggerFactory));

            _services.ConfigureIdempotency(
                _configuration.GetSection(IdempotencyConfiguration.Section).Get<IdempotencyConfiguration>().ConnectionString,
                new IdempotencyMigrationsTableInfo(Schema.Import),
                new IdempotencyTableInfo(Schema.Import),
                _loggerFactory);

            builder.RegisterSnapshotModule(_configuration);

            // Authorization
            _services.AddAcmIdmAuthorizationHandlers();

            var ovoCodeWhiteList = _configuration.GetSection("OvoCodeWhiteList").Get<List<string>>();

            if (_environment.IsDevelopment())
            {
                _services.AddHardCodedNisCodeService();
            }
            else
            {
                _services.AddDynamoDbNisCodeService();
            }

            _services
                .AddNisCodeAuthorization<PersistentLocalId, StreetNameRegistryNisCodeFinder>()
                .AddNisCodeAuthorization<MunicipalityPuri, MunicipalityRegistryNisCodeFinder>()
                .AddOvoCodeWhiteList(ovoCodeWhiteList);

            builder.Populate(_services);
        }
    }
}
