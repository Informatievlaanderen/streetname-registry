namespace StreetNameRegistry.Api.BackOffice.Infrastructure.Authorization
{
    using Microsoft.Extensions.DependencyInjection;
    using Municipality;
    using NisCodeService.DynamoDb.Extensions;
    using NisCodeService.HardCoded.Extensions;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNisCodeAuthorizationWithDynamoDb(this IServiceCollection services)
        {
            services.AddSingleton<INisCodeAuthorizer<PersistentLocalId>, NisCodeAuthorizer<PersistentLocalId>>();
            services.AddSingleton<INisCodeAuthorizer<MunicipalityPuri>, NisCodeAuthorizer<MunicipalityPuri>>();
            services.AddSingleton<INisCodeFinder<PersistentLocalId>, StreetNameNisCodeFinder>();
            services.AddSingleton<INisCodeFinder<MunicipalityPuri>, MunicipalityNisCodeFinder>();
            services.AddDynamoDbNisCodeService();
            return services;
        }

        public static IServiceCollection AddNisCodeAuthorizationHardCoded(this IServiceCollection services)
        {
            services.AddSingleton<INisCodeAuthorizer<PersistentLocalId>, NisCodeAuthorizer<PersistentLocalId>>();
            services.AddSingleton<INisCodeAuthorizer<MunicipalityId>, NisCodeAuthorizer<MunicipalityId>>();
            services.AddSingleton<INisCodeFinder<PersistentLocalId>, StreetNameNisCodeFinder>();
            services.AddHardCodedNisCodeService();
            return services;
        }
    }
}
