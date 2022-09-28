namespace StreetNameRegistry.Tests.BackOffice.Api.WhenApprovingStreetName
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
    using StreetNameRegistry.Api.BackOffice;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Exceptions;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenMunicipalityExists : BackOfficeApiTest<StreetNameController>
    {
        public GivenMunicipalityExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper, useSqs: true)
        {
        }

        [Fact]
        public async Task ThenAcceptedWithLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));
            var expectedIfMatchHeader = Fixture.Create<string>();

            MockMediatorResponse<SqsStreetNameApproveRequest, LocationResult>(expectedLocationResult);
            var request = new StreetNameApproveRequest { PersistentLocalId = 123 };


            var result = (AcceptedResult)await Controller.Approve(
                MockValidIfMatchValidator(),
                MockPassingRequestValidator<StreetNameApproveRequest>(),
                request,
                ifMatchHeaderValue: expectedIfMatchHeader,
                CancellationToken.None);

            // Assert
            MockMediator.Verify(x =>
                x.Send(
                    It.Is<SqsStreetNameApproveRequest>(sqsRequest =>
                        sqsRequest.Request == request &&
                        sqsRequest.ProvenanceData.Timestamp != Instant.MinValue && // Just to verify that ProvenanceData has been populated.
                        sqsRequest.IfMatchHeaderValue == expectedIfMatchHeader),
                    CancellationToken.None));

            AssertLocation(result.Location, ticketId);
        }

        [Fact]
        public void WithAggregateIdIsNotFound_ThenThrowsApiException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<SqsStreetNameApproveRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            var request = new StreetNameApproveRequest { PersistentLocalId = 123 };
            Func<Task> act = async () =>
            {
                await Controller.Approve(
                    MockValidIfMatchValidator(),
                    MockPassingRequestValidator<StreetNameApproveRequest>(),
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
        public async Task WithIfMatchHeaderValueMismatch_ThenReturnsPreconditionFailedResult()
        {
            var result = await Controller.Approve(
                MockValidIfMatchValidator(false),
                MockPassingRequestValidator<StreetNameApproveRequest>(),
                new StreetNameApproveRequest { PersistentLocalId = 123 },
                string.Empty,
                CancellationToken.None);

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }
    }
}
