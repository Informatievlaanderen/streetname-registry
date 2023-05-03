namespace StreetNameRegistry.Tests.BackOffice.Api.WhenRetiringStreetName
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Municipality;
    using StreetNameRegistry.Api.BackOffice;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using NodaTime;
    using StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenMunicipalityExists : BackOfficeApiTest<StreetNameController>
    {

        public GivenMunicipalityExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public async Task ThenAcceptedWithLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));
            var expectedIfMatchHeader = Fixture.Create<string>();
            MockMediatorResponse<RetireStreetNameSqsRequest, LocationResult>(expectedLocationResult);

            var request = new RetireStreetNameRequest
            {
                PersistentLocalId = 123
            };

            var result = (AcceptedResult) await Controller.Retire(
                MockNisCodeAuthorizer<PersistentLocalId>(),
                MockValidIfMatchValidator(),
                request,
                ifMatchHeaderValue: expectedIfMatchHeader,
                CancellationToken.None);

            // Assert
            MockMediator.Verify(x =>
                x.Send(
                    It.Is<RetireStreetNameSqsRequest>(sqsRequest =>
                        sqsRequest.Request == request &&
                        sqsRequest.ProvenanceData.Timestamp != Instant.MinValue &&
                        sqsRequest.ProvenanceData.Application == Application.StreetNameRegistry &&
                        sqsRequest.ProvenanceData.Modification == Modification.Update &&
                        sqsRequest.IfMatchHeaderValue == expectedIfMatchHeader),
                    CancellationToken.None));
            AssertLocation(result.Location, ticketId);
        }

        [Fact]
        public void WithAggregateIdIsNotFound_ThenThrowsApiException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<RetireStreetNameSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            var request = new RetireStreetNameRequest { PersistentLocalId = 123 };
            Func<Task> act = async () =>
            {
                await Controller.Retire(
                    MockNisCodeAuthorizer<PersistentLocalId>(),
                    MockValidIfMatchValidator(),
                    request,
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
        public void WithAggregateNotFound_ThenThrowsApiException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<RetireStreetNameSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateNotFoundException("test", typeof(Municipality)));

            var request = new RetireStreetNameRequest { PersistentLocalId = 123 };
            Func<Task> act = async () =>
            {
                await Controller.Retire(
                    MockNisCodeAuthorizer<PersistentLocalId>(),
                    MockValidIfMatchValidator(),
                    request,
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
        public void WithUnauthorizedNisCode_ThenThrowsApiException()
        {
            var request = new RetireStreetNameRequest { PersistentLocalId = 123 };
            Func<Task> act = async () =>
            {
                await Controller.Retire(
                    MockNisCodeAuthorizer<PersistentLocalId>(false),
                    MockValidIfMatchValidator(),
                    request,
                    string.Empty,
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

        [Fact]
        public async Task WithIfMatchHeaderValueMismatch_ThenReturnsPreconditionFailedResult()
        {
            var result = await Controller.Retire(
                MockNisCodeAuthorizer<PersistentLocalId>(),
                MockValidIfMatchValidator(false),
                new RetireStreetNameRequest { PersistentLocalId = 123 },
                string.Empty,
                CancellationToken.None);

            result.Should().BeOfType<PreconditionFailedResult>();
        }
    }
}
