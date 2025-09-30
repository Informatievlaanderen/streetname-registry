namespace StreetNameRegistry.Projections.Elastic
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;
    using StreetNameRegistry.Infrastructure;

    public class ElasticRunnerContext : RunnerDbContext<ElasticRunnerContext>
    {
        public override string ProjectionStateSchema => Schema.Elastic;

        public ElasticRunnerContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ElasticRunnerContext(DbContextOptions<ElasticRunnerContext> options)
            : base(options) { }
    }
}
