namespace StreetNameRegistry.Tests.BackOffice.Api.WhenProposingStreetName
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Municipality;
    using NodaTime;
    using StreetNameRegistry.Api.BackOffice;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenMunicipalityExists : BackOfficeApiTest<StreetNameController>
    {
        public GivenMunicipalityExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task ThenAcceptedWithLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));
            MockMediatorResponse<ProposeStreetNameSqsRequest, LocationResult>(expectedLocationResult);

            var request = new ProposeStreetNameRequest
            {
                GemeenteId = GetStreetNamePuri(123),
                Straatnamen = new Dictionary<Taal, string>
                {
                    {Taal.NL, "Rodekruisstraat"},
                    {Taal.FR, "Rue de la Croix-Rouge"}
                }
            };

            var result = (AcceptedResult) await Controller.Propose(
                MockPassingRequestValidator<ProposeStreetNameRequest>(),
                new ProposeStreetNameRequestFactory(new FakePersistentLocalIdGenerator()),
                request,
                CancellationToken.None);

            // Assert
            MockMediator.Verify(x =>
                x.Send(
                    It.Is<ProposeStreetNameSqsRequest>(sqsRequest =>
                        sqsRequest.Request == request &&
                        sqsRequest.ProvenanceData.Timestamp != Instant.MinValue &&
                        sqsRequest.ProvenanceData.Application == Application.StreetNameRegistry &&
                        sqsRequest.ProvenanceData.Modification == Modification.Insert),
                    CancellationToken.None));
            AssertLocation(result.Location, ticketId);
        }
    }
}
