namespace StreetNameRegistry.Tests.BackOffice.Api.WhenRenamingStreetName
{
    using System;
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
    using NodaTime;
    using StreetNameRegistry.Api.BackOffice;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests;
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
            var expectedIfMatchHeader = Fixture.Create<string>();
            MockMediatorResponse<RenameStreetNameSqsRequest, LocationResult>(expectedLocationResult);

            var request = new RenameStreetNameRequest
            {
                DoelStraatnaamId = "https://data.vlaanderen.be/id/straatnaam/123"
            };

            var result = (AcceptedResult)await Controller.Rename(
                MockValidIfMatchValidator(),
                MockPassingRequestValidator<RenameStreetNameRequest>(),
                456,
                request,
                ifMatchHeaderValue: expectedIfMatchHeader,
                CancellationToken.None);

            // Assert
            MockMediator.Verify(x =>
                x.Send(
                    It.Is<RenameStreetNameSqsRequest>(sqsRequest =>
                        sqsRequest.Request == request &&
                        sqsRequest.PersistentLocalId == 456 &&
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
                .Setup(x => x.Send(It.IsAny<RenameStreetNameSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            var request = new RenameStreetNameRequest
            {
                DoelStraatnaamId = "https://data.vlaanderen.be/id/straatnaam/123"
            };

            Func<Task> act = async () =>
            {
                await Controller.Rename(
                    MockValidIfMatchValidator(),
                    MockPassingRequestValidator<RenameStreetNameRequest>(),
                    456,
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
                .Setup(x => x.Send(It.IsAny<RenameStreetNameSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateNotFoundException("test", typeof(Municipality)));

            var request = new RenameStreetNameRequest
            {
                DoelStraatnaamId = "https://data.vlaanderen.be/id/straatnaam/123"
            };

            Func<Task> act = async () =>
            {
                await Controller.Rename(
                    MockValidIfMatchValidator(),
                    MockPassingRequestValidator<RenameStreetNameRequest>(),
                    456,
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
            var result = await Controller.Rename(
                MockValidIfMatchValidator(false),
                MockPassingRequestValidator<RenameStreetNameRequest>(),
                456,
                new RenameStreetNameRequest
                {
                    DoelStraatnaamId = "https://data.vlaanderen.be/id/straatnaam/123"
                },
                string.Empty,
                CancellationToken.None);

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }
    }
}