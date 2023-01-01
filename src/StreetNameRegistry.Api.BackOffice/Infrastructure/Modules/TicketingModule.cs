namespace StreetNameRegistry.Api.BackOffice.Infrastructure.Modules
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using TicketingService.Abstractions;
    using TicketingService.Proxy.HttpProxy;
    using Module = Autofac.Module;

    public sealed class TicketingModule : Module, IServiceCollectionModule
    {
        internal const string TicketingServiceConfigKey = "TicketingService";

        private readonly string _baseUrl;

        public TicketingModule(
            IConfiguration configuration,
            IServiceCollection services)
        {
            _baseUrl = configuration.GetSection(TicketingServiceConfigKey)["InternalBaseUrl"];
            services
                .AddHttpProxyTicketing(_baseUrl);
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c => new TicketingUrl(_baseUrl))
                .As<ITicketingUrl>()
                .SingleInstance();
        }

        public void Load(IServiceCollection services)
        {
            services.AddSingleton<ITicketingUrl>(_ => new TicketingUrl(_baseUrl));
        }
    }
}
