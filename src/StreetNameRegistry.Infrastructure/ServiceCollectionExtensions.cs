namespace StreetNameRegistry.Infrastructure
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore.Microsoft;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.SqlStreamStore.Microsoft;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterEventstreamModule(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Events");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Missing 'Events' connectionstring.");
            }

            services
                .RegisterModule(new SqlStreamStoreModule(connectionString, Schema.Default))
                .RegisterModule(new TraceSqlStreamStoreModule(configuration["DataDog:ServiceName"]));

            return services;
        }

        public static IServiceCollection RegisterSnapshotModule(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Snapshots");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Missing 'Snapshots' connectionstring.");
            }

            services.RegisterModule(new SqlSnapshotStoreModule(connectionString, Schema.Default));

            return services;
        }
    }
}
