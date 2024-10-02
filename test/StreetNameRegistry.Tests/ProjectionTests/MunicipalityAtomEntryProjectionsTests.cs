namespace StreetNameRegistry.Tests.ProjectionTests
{
    using System;
    using Generate;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Projections.Syndication;
    using Projections.Syndication.Municipality;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class MunicipalityAtomEntryProjectionsTests : SyndicationProjectionTest<SyndicationContext, MunicipalityEvent, SyndicationContent<Gemeente>, MunicipalitySyndicationProjections>
    {
        public MunicipalityAtomEntryProjectionsTests(ITestOutputHelper output) : base(output)
        { }

        protected override SyndicationContext CreateContext(DbContextOptions<SyndicationContext> options) => new SyndicationContext(options);

        [Theory]
        [InlineData(MunicipalityEvent.MunicipalityWasRegistered)]
        [InlineData(MunicipalityEvent.MunicipalityWasNamed)]
        [InlineData(MunicipalityEvent.MunicipalityNameWasCleared)]
        [InlineData(MunicipalityEvent.MunicipalityNameWasCorrected)]
        [InlineData(MunicipalityEvent.MunicipalityNameWasCorrectedToCleared)]
        [InlineData(MunicipalityEvent.MunicipalityNisCodeWasDefined)]
        [InlineData(MunicipalityEvent.MunicipalityNisCodeWasCorrected)]
        [InlineData(MunicipalityEvent.MunicipalityWasMerged)]
        public async Task MunicipalityEventInsertsRecord(MunicipalityEvent @event)
        {
            var id = Arrange(Produce.Guid());
            var position = Arrange(Produce.Integer(100, 6000));

            await Given()
                .Project(Generate.AtomEntry(@event, position)
                    .Select(e => e
                        .WithObjectId<Gemeente>(id))
                    .Generate(new Random()))
                .Then(async context =>
                {
                    var entry = await context.FindAsync<MunicipalitySyndicationItem>(id, (long)position);

                    entry.Should().NotBeNull();
                    entry!.MunicipalityId.Should().Be(id);
                    entry.Position.Should().Be(position);
                });
        }
    }
}
