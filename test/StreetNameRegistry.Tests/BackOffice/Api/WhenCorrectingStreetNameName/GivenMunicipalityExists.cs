namespace StreetNameRegistry.Tests.BackOffice.Api.WhenCorrectingStreetNameName
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using StreetNameRegistry.Api.BackOffice;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using NodaTime;
    using StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenMunicipalityExists : BackOfficeApiTest<StreetNameController>
    {
        public GivenMunicipalityExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper, useSqs: true)
        { }

        [Fact]
        public async Task ThenAcceptedWithLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));
            var expectedIfMatchHeader = Fixture.Create<string>();
            MockMediatorResponse<CorrectStreetNameNamesSqsRequest, LocationResult>(expectedLocationResult);

            var request = new StreetNameCorrectNamesRequest
            {
                Straatnamen = new Dictionary<Taal, string> { { Taal.NL, "Bosstraat" } }
            };

            var persistentLocalId = 123;
            var result = (AcceptedResult) await Controller.CorrectStreetNameNames(
                MockValidIfMatchValidator(),
                MockPassingRequestValidator<StreetNameCorrectNamesRequest>(),
                ResponseOptions,
                persistentLocalId,
                request,
                ifMatchHeaderValue: expectedIfMatchHeader,
                CancellationToken.None);

            // Assert
            MockMediator.Verify(x =>
                x.Send(
                    It.Is<CorrectStreetNameNamesSqsRequest>(sqsRequest =>
                        sqsRequest.Request == request &&
                        sqsRequest.PersistentLocalId == persistentLocalId &&
                        sqsRequest.ProvenanceData.Timestamp != Instant.MinValue && // Just to verify that ProvenanceData has been populated.
                        sqsRequest.IfMatchHeaderValue == expectedIfMatchHeader),
                    CancellationToken.None));

            AssertLocation(result.Location, ticketId);
        }

        [Fact]
        public void WithAggregateIdIsNotFound_ThenThrowsApiException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectStreetNameNamesSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            var persistentLocalId = 123;
            Func<Task> act = async () =>
            {
                await Controller.CorrectStreetNameNames(
                    MockValidIfMatchValidator(),
                    MockPassingRequestValidator<StreetNameCorrectNamesRequest>(),
                    ResponseOptions,
                    persistentLocalId,
                    new StreetNameCorrectNamesRequest(),
                    string.Empty,
                    CancellationToken.None);
            };

            //Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message.Contains("Onbestaande straatnaam.")
                    && x.StatusCode == StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task WithIfMatchHeaderValueMismatch_ThenReturnsPreconditionFailedResult()
        {
            var result = await Controller.CorrectStreetNameNames(
                MockValidIfMatchValidator(false),
                MockPassingRequestValidator<StreetNameCorrectNamesRequest>(),
                ResponseOptions,
                123,
                new StreetNameCorrectNamesRequest(),
                string.Empty,
                CancellationToken.None);

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }
    }
}
