namespace StreetNameRegistry.Consumer
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer;
    using Microsoft.EntityFrameworkCore;
    using Municipality;
    using StreetNameRegistry.Infrastructure;

    public class ConsumerContext : RunnerDbContext<ConsumerContext>
    {
        public DbSet<MunicipalityConsumerItem> MunicipalityConsumerItems { get; set; }

        // This needs to be here to please EF
        public ConsumerContext()
        { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ConsumerContext(DbContextOptions<ConsumerContext> options)
            : base(options)
        { }

        public override string ProjectionStateSchema => Schema.ConsumerProjections;
    }

    public class ConsumerContextFactory : SqlServerRunnerDbContextMigrationFactory<ConsumerContext>
    {
        public ConsumerContextFactory()
            : this("ConsumerAdmin")
        { }

        public ConsumerContextFactory(string connectionStringName)
            : base(connectionStringName, new MigrationHistoryConfiguration
            {
                Schema = Schema.ConsumerProjections,
                Table = MigrationTables.ConsumerProjections
            })
        { }

        protected override ConsumerContext CreateContext(DbContextOptions<ConsumerContext> migrationContextOptions)
            => new ConsumerContext(migrationContextOptions);

        public ConsumerContext Create(DbContextOptions<ConsumerContext> options) => CreateContext(options);
    }
}
