namespace StreetNameRegistry.Tests.BackOffice.Api.WhenCorrectingStreetNameHomonymAdditions
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
    using StreetNameRegistry.Api.BackOffice;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using Municipality;
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
            MockMediatorResponse<CorrectStreetNameHomonymAdditionsSqsRequest, LocationResult>(expectedLocationResult);

            var request = new CorrectStreetNameHomonymAdditionsRequest()
            {
                HomoniemToevoegingen = new Dictionary<Taal, string?>()
                {
                    { Taal.DE , "ABC"}
                }
            };

            var persistentLocalId = 123;
            var result = (AcceptedResult) await Controller.CorrectHomonymAddition(
                MockValidIfMatchValidator(),
                MockPassingRequestValidator<CorrectStreetNameHomonymAdditionsRequest>(),
                persistentLocalId,
                request,
                ifMatchHeaderValue: expectedIfMatchHeader,
                CancellationToken.None);

            // Assert
            MockMediator.Verify(x =>
                x.Send(
                    It.Is<CorrectStreetNameHomonymAdditionsSqsRequest>(sqsRequest =>
                        sqsRequest.PersistentLocalId == persistentLocalId &&
                        sqsRequest.Request == request &&
                        sqsRequest.PersistentLocalId == persistentLocalId &&
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
                .Setup(x => x.Send(It.IsAny<CorrectStreetNameHomonymAdditionsSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            var persistentLocalId = 123;
            Func<Task> act = async () =>
            {
                await Controller.CorrectHomonymAddition(
                    MockValidIfMatchValidator(),
                    MockPassingRequestValidator<CorrectStreetNameHomonymAdditionsRequest>(),
                    persistentLocalId,
                    new CorrectStreetNameHomonymAdditionsRequest(),
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
                .Setup(x => x.Send(It.IsAny<CorrectStreetNameHomonymAdditionsSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateNotFoundException("test", typeof(Municipality)));

            var persistentLocalId = 123;
            Func<Task> act = async () =>
            {
                await Controller.CorrectHomonymAddition(
                    MockValidIfMatchValidator(),
                    MockPassingRequestValidator<CorrectStreetNameHomonymAdditionsRequest>(),
                    persistentLocalId,
                    new CorrectStreetNameHomonymAdditionsRequest(),
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
            var result = await Controller.CorrectHomonymAddition(
                MockValidIfMatchValidator(false),
                MockPassingRequestValidator<CorrectStreetNameHomonymAdditionsRequest>(),
                123,
                new CorrectStreetNameHomonymAdditionsRequest(),
                string.Empty,
                CancellationToken.None);

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }
    }
}
