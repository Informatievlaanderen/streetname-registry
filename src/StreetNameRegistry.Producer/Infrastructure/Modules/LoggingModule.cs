namespace StreetNameRegistry.Producer.Infrastructure.Modules
{
    using System;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Destructurama;
    using global::Microsoft.Extensions.Configuration;
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Debugging;

    public class LoggingModule : Module, IServiceCollectionModule
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;

        public LoggingModule(
            IConfiguration configuration,
            IServiceCollection services)
        {
            _configuration = configuration;
            _services = services;
        }

        protected override void Load(ContainerBuilder builder)
        {
            SelfLog.Enable(Console.WriteLine);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentUserName()
                .Destructure.JsonNetTypes()
                .CreateLogger();

            _services.AddLogging(l =>
            {
                l.ClearProviders();
                l.AddSerilog(Log.Logger);
            });
        }

        public void Load(IServiceCollection services)
        {
            SelfLog.Enable(Console.WriteLine);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentUserName()
                .Destructure.JsonNetTypes()
                .CreateLogger();

            services.AddLogging(l =>
            {
                l.ClearProviders();
                l.AddSerilog(Log.Logger);
            });
        }
    }
}
