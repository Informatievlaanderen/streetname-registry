namespace StreetNameRegistry.Tests.BackOffice.Api.WhenProposingStreetName
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
    using StreetNameRegistry.Api.BackOffice;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using StreetNameRegistry.Api.BackOffice.Infrastructure.Authorization;
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
                MockNisCodeAuthorizer<MunicipalityPuri>(),
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

        [Fact]
        public void WithUnauthorizedNisCode_ThenThrowsApiException()
        {
            var request = new ProposeStreetNameRequest { GemeenteId = "123" };
            Func<Task> act = async () =>
            {
                await Controller.Propose(
                    MockNisCodeAuthorizer<MunicipalityPuri>(false),
                    MockPassingRequestValidator<ProposeStreetNameRequest>(),
                    new ProposeStreetNameRequestFactory(new FakePersistentLocalIdGenerator()),
                    request,
                    CancellationToken.None);
            };

            //Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message.Contains("User has insufficient privileges to make edit changes on the municipality.")
                    && x.StatusCode == (int)HttpStatusCode.Forbidden);
        }
    }
}
