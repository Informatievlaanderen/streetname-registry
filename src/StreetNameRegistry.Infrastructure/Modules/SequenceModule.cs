namespace StreetNameRegistry.Infrastructure.Modules
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Municipality;

    public class SequenceModule : Module, IServiceCollectionModule
    {
        public SequenceModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var projectionsConnectionString = configuration.GetConnectionString("Sequences");

            services
                .AddDbContext<SequenceContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(projectionsConnectionString, sqlServerOptions => sqlServerOptions
                            .EnableRetryOnFailure()
                            .MigrationsHistoryTable(MigrationTables.Sequence, Schema.Sequence)
                    ));
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<SqlPersistentLocalIdGenerator>()
                .As<IPersistentLocalIdGenerator>();
        }

        public void Load(IServiceCollection services)
        {
            services.AddTransient<IPersistentLocalIdGenerator, SqlPersistentLocalIdGenerator>();
        }
    }
}
