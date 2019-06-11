namespace StreetNameRegistry.Tests.ProjectionTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using FluentAssertions;
    using Generate;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using NodaTime;
    using Projections.Legacy;
    using Projections.Legacy.StreetNameVersion;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using StreetName.Events;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public class StreetNameVersionItemProjectionsTests : StreetNameRegistryProjectionTest<StreetNameVersionProjections>
    {
        public StreetNameVersionItemProjectionsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task StreetNameVersionWasIncreasedCreatesRecordWithCorrectProvenance()
        {
            var id = Arrange(Generate.StreetNameId);

            var streetNameWasRegistered = Arrange(Generate.StreetNameWasRegistered.Select(e => e.WithId(id)));
            var provenance = new Provenance(Instant.FromDateTimeOffset(DateTimeOffset.Now), Application.CrabWstEditService, Reason.DecentralManagmentCrab, new Operator("test"), Modification.Update, Organisation.Municipality);
            ((ISetProvenance)streetNameWasRegistered).SetProvenance(provenance);

            var projection = new StreetNameVersionProjections();
            var resolver = ConcurrentResolve.WhenEqualToHandlerMessageType(projection.Handlers);

            var metadata = new Dictionary<string, object>
            {
                [Provenance.ProvenanceMetadataKey] = JsonConvert.SerializeObject(provenance.ToDictionary()),
                [Envelope.PositionMetadataKey] = 1L
            };

            await new ConnectedProjectionScenario<LegacyContext>(resolver)
                .Given(new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, metadata)))
                .Verify(async context =>
                {
                    var streetNameVersion = await context.StreetNameVersions
                        .FirstAsync(a => a.StreetNameId == id);

                    streetNameVersion.Should().NotBeNull();

                    streetNameVersion.Reason.Should().Be(provenance.Reason);
                    streetNameVersion.Application.Should().Be(provenance.Application);
                    streetNameVersion.VersionTimestamp.Should().Be(provenance.Timestamp);
                    streetNameVersion.Operator.Should().Be(provenance.Operator.ToString());
                    streetNameVersion.Modification.Should().Be(provenance.Modification);
                    streetNameVersion.Organisation.Should().Be(provenance.Organisation);

                    return VerificationResult.Pass();
                })
                .Assert();
        }
    }
}
