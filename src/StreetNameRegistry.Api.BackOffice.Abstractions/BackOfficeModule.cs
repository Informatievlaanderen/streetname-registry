namespace StreetNameRegistry.Api.BackOffice.Abstractions
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public sealed class BackOfficeModule : Module, IServiceCollectionModule
    {
        public BackOfficeModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var projectionsConnectionString = configuration.GetConnectionString("BackOffice");

            services
                .AddDbContext<BackOfficeContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(projectionsConnectionString, sqlServerOptions => sqlServerOptions
                            .EnableRetryOnFailure()
                            .MigrationsHistoryTable(MigrationTables.BackOffice, Schema.BackOffice)
                    ));
        }

        public void Load(IServiceCollection services)
        {
            // ignore
        }
    }
}
