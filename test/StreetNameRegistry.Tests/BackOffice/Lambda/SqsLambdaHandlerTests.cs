namespace StreetNameRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using global::AutoFixture;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Municipality;
    using Municipality.Exceptions;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class SqsLambdaHandlerTests : BackOfficeLambdaTest
    {
        public SqsLambdaHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task TicketShouldBeUpdatedToPendingAndCompleted()
        {
            var ticketing = new Mock<ITicketing>();
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();

            var sqsLambdaRequest = new ApproveStreetNameLambdaRequest(Guid.NewGuid().ToString(), new ApproveStreetNameSqsRequest
            {
                Request = new ApproveStreetNameRequest {PersistentLocalId = 1},
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            });

            var sut = new FakeLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<IMunicipalities>(),
                ticketing.Object,
                idempotentCommandHandler.Object);

            await sut.Handle(sqsLambdaRequest, CancellationToken.None);

            ticketing.Verify(x => x.Pending(sqsLambdaRequest.TicketId, CancellationToken.None), Times.Once);
            ticketing.Verify(
                x => x.Complete(sqsLambdaRequest.TicketId,
                    new TicketResult(new ETagResponse("bla", "etag")), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenStreetNameIsNotFoundException_ThenTicketingErrorIsExpected()
        {
            var ticketing = new Mock<ITicketing>();

            var sqsLambdaRequest = new ApproveStreetNameLambdaRequest(Guid.NewGuid().ToString(), new ApproveStreetNameSqsRequest
            {
                Request = new ApproveStreetNameRequest(),
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            });

            var sut = new FakeLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<IMunicipalities>(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<StreetNameIsNotFoundException>().Object);

            await sut.Handle(sqsLambdaRequest, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(sqsLambdaRequest.TicketId, new TicketError("Onbestaande straatnaam.", "OnbestaandeStraatnaam"),
                    CancellationToken.None));
            ticketing.Verify(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), CancellationToken.None),
                Times.Never);
        }

        [Fact]
        public async Task WhenStreetNameIsRemovedException_ThenTicketingErrorIsExpected()
        {
            var ticketing = new Mock<ITicketing>();

            var sqsLambdaRequest = new ApproveStreetNameLambdaRequest(Guid.NewGuid().ToString(), new ApproveStreetNameSqsRequest
            {
                Request = new ApproveStreetNameRequest(),
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            });

            var sut = new FakeLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<IMunicipalities>(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<StreetNameIsRemovedException>().Object);

            await sut.Handle(sqsLambdaRequest, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(sqsLambdaRequest.TicketId, new TicketError("Verwijderde straatnaam.", "VerwijderdeStraatnaam"),
                    CancellationToken.None));
            ticketing.Verify(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), CancellationToken.None),
                Times.Never);
        }

        [Fact]
        public async Task WhenIfMatchHeaderValueIsMismatch_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var municipalityId = new MunicipalityId(Guid.NewGuid());
            var streetNamePersistentLocalId = new PersistentLocalId(456);

            ImportMunicipality(municipalityId, new NisCode("23002"));
            SetMunicipalityToCurrent(municipalityId);
            AddOfficialLanguageDutch(municipalityId);
            ProposeStreetName(
                municipalityId,
                new Names(new Dictionary<Language, string> { { Language.Dutch, "Bremt" } }),
                streetNamePersistentLocalId,
                Fixture.Create<Provenance>());

            var sut = new FakeLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Container.Resolve<IMunicipalities>(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object);

            // Act
            await sut.Handle(new ApproveStreetNameLambdaRequest(municipalityId, new ApproveStreetNameSqsRequest
            {
                IfMatchHeaderValue = "Outdated",
                Request = new ApproveStreetNameRequest
                {
                    PersistentLocalId = streetNamePersistentLocalId
                },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError("Als de If-Match header niet overeenkomt met de laatste ETag.", "PreconditionFailed"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenNoIfMatchHeaderValueIsPresent_ThenInnerHandleIsCalled()
        {
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();

            var sqsLambdaRequest = new ApproveStreetNameLambdaRequest(Guid.NewGuid().ToString(), new ApproveStreetNameSqsRequest
            {
                Request = new ApproveStreetNameRequest(),
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            });

            var sut = new FakeLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<IMunicipalities>(),
                Mock.Of<ITicketing>(),
                idempotentCommandHandler.Object);

            await sut.Handle(sqsLambdaRequest, CancellationToken.None);

            //Assert
            idempotentCommandHandler
                .Verify(x =>
                    x.Dispatch(It.IsAny<Guid>(), It.IsAny<object>(), It.IsAny<IDictionary<string, object>>(), new CancellationToken()),
                    Times.Once);
        }
    }

    public sealed class FakeLambdaHandler : StreetNameLambdaHandler<ApproveStreetNameLambdaRequest>
    {
        public FakeLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            IMunicipalities municipalities,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler)
            : base(
                configuration,
                retryPolicy,
                municipalities,
                ticketing,
                idempotentCommandHandler)
        { }

        protected override Task<object> InnerHandle(ApproveStreetNameLambdaRequest request, CancellationToken cancellationToken)
        {
            IdempotentCommandHandler.Dispatch(
                Guid.NewGuid(),
                new object(),
                new Dictionary<string, object>(),
                cancellationToken);

            return Task.FromResult((object) new ETagResponse("bla", "etag"));
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, ApproveStreetNameLambdaRequest request)
        {
            return null;
        }
    }
}
