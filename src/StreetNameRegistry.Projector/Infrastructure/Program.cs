namespace StreetNameRegistry.Projector.Infrastructure
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Consumer.Infrastructure.Modules;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using Serilog.Extensions.Logging;
    using LoggingModule = Modules.LoggingModule;

    public class Program
    {
        protected Program()
        { }

        public static void Main(string[] args)
            => Run(new ProgramOptions
                {
                    Hosting =
                    {
                        HttpPort = 4006
                    },
                    Logging =
                    {
                        WriteTextToConsole = false,
                        WriteJsonToConsole = false
                    },
                    Runtime =
                    {
                        CommandLineArgs = args
                    },
                    MiddlewareHooks =
                    {
                        ConfigureDistributedLock =
                            DistributedLockOptions.LoadFromConfiguration
                    }
                });

        private static void Run(ProgramOptions options)
            => new HostBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((hostContext, containerBuilder) =>
                {
                    var services = new ServiceCollection();
                    var logger = new SerilogLoggerFactory(Log.Logger);
                    containerBuilder
                        .RegisterModule(new ApiModule(hostContext.Configuration, services, logger))
                        .RegisterModule(new LoggingModule(hostContext.Configuration, services));
                })
                .UseDefaultForApi<Startup>(options)
                .RunWithLock<Program>();
    }
}
