namespace StreetNameRegistry.Producer.Microsoft
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.Microsoft;
    using global::Microsoft.EntityFrameworkCore;
    using StreetNameRegistry.Infrastructure;

    public class ProducerContext : RunnerDbContext<ProducerContext>
    {
        public override string ProjectionStateSchema => Schema.Producer;

        // This needs to be here to please EF
        public ProducerContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ProducerContext(DbContextOptions<ProducerContext> options)
            : base(options) { }
    }
}
