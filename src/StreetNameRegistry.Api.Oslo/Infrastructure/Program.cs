namespace StreetNameRegistry.Api.Oslo.Infrastructure
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Modules;
    using Serilog;
    using Serilog.Extensions.Logging;
    using ApiModule = Consumer.Infrastructure.Modules.ApiModule;

    public static class Program
    {
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args)
            => new HostBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((hostContext, containerBuilder) =>
                {
                    var services = new ServiceCollection();
                    var logger = new SerilogLoggerFactory(Log.Logger);
                    containerBuilder
                        .RegisterModule(new ApiModule(hostContext.Configuration, services, logger))
                        .RegisterModule(new ElasticModule(hostContext.Configuration))
                        .RegisterModule(new MediatRModule());
                })
                .UseDefaultForApi<Startup>(
                    new ProgramOptions
                    {
                        Hosting =
                        {
                            HttpPort = 4012
                        },
                        Logging =
                        {
                            WriteTextToConsole = false,
                            WriteJsonToConsole = false
                        },
                        Runtime =
                        {
                            CommandLineArgs = args
                        }
                    });
    }
}
