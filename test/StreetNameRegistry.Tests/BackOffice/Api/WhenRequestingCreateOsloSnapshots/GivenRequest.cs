namespace StreetNameRegistry.Tests.BackOffice.Api.WhenRequestingCreateOsloSnapshots
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Municipality;
    using NodaTime;
    using StreetNameRegistry.Api.BackOffice;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenRequest : BackOfficeApiTest<StreetNameController>
    {
        public GivenRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var persistentLocalIds = Fixture.CreateMany<PersistentLocalId>();

            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(
                    It.IsAny<CreateOsloSnapshotsSqsRequest>(),
                    CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var request = new CreateOsloSnapshotsRequest
            {
                PersistentLocalIds = persistentLocalIds.Select(x => (int)x).ToList(),
                Reden = "UnitTest"
            };

            var result = (AcceptedResult)await Controller.CreateOsloSnapshots(request);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<CreateOsloSnapshotsSqsRequest>(sqsRequest =>
                        sqsRequest.Request == request
                        && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.StreetNameRegistry
                        && sqsRequest.ProvenanceData.Modification == Modification.Unknown
                        && sqsRequest.ProvenanceData.Reason == request.Reden
                    ),
                    CancellationToken.None));
        }
    }
}
