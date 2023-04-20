namespace StreetNameRegistry.Api.BackOffice.Infrastructure.Authorization
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Municipality;
    using NisCodeService.DynamoDb.Extensions;
    using NisCodeService.HardCoded.Extensions;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNisCodeAuthorizationWithDynamoDb(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .RegisterCommonServices(configuration)
                .AddDynamoDbNisCodeService();

            return services;
        }

        public static IServiceCollection AddNisCodeAuthorizationHardCoded(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .RegisterCommonServices(configuration)
                .AddHardCodedNisCodeService();

            return services;
        }

        private static IServiceCollection RegisterCommonServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<INisCodeAuthorizer<PersistentLocalId>, NisCodeAuthorizer<PersistentLocalId>>();
            services.AddSingleton<INisCodeAuthorizer<MunicipalityPuri>, NisCodeAuthorizer<MunicipalityPuri>>();
            services.AddSingleton<INisCodeFinder<PersistentLocalId>, StreetNameNisCodeFinder>();
            services.AddSingleton<INisCodeFinder<MunicipalityPuri>, MunicipalityNisCodeFinder>();

            services.Configure<OvoCodeWhiteListOptions>(configuration);
            services.AddSingleton<IOvoCodeWhiteList, OvoCodeWhiteList>();

            return services;
        }
    }
}
