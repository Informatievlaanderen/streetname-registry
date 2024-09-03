namespace StreetNameRegistry.Tests.BackOffice.Lambda.WhenCreatingOsloSnapshotsRequest
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AllStream;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
    using Moq;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenMunicipalityExists : BackOfficeLambdaTest
    {
        private readonly IdempotencyContext _idempotencyContext;

        public GivenMunicipalityExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext([]);
        }

        [Fact]
        public async Task ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var handler = new CreateOsloSnapshotsLambdaHandler(
                new FakeRetryPolicy(),
                ticketing.Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext));

            // Act
            var ticketId = Guid.NewGuid();
            await handler.Handle(
                new CreateOsloSnapshotsLambdaRequest(
                    AllStreamId.Instance,
                    new CreateOsloSnapshotsSqsRequest
                    {
                        TicketId = ticketId,
                        Request = new CreateOsloSnapshotsRequest
                        {
                            PersistentLocalIds = [1]
                        },
                        ProvenanceData = Fixture.Create<ProvenanceData>()
                    }),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Complete(
                    ticketId,
                    new TicketResult("done"),
                    CancellationToken.None));

            //Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(AllStreamId.Instance), 0, 1);
            var message = stream.Messages.First();
            message.JsonMetadata.Should().Contain(Provenance.ProvenanceMetadataKey.ToLower());
        }

        [Fact]
        public async Task WhenIdempotencyException_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var handler = new CreateOsloSnapshotsLambdaHandler(
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object);

            // Act
            var ticketId = Guid.NewGuid();
            await handler.Handle(
                new CreateOsloSnapshotsLambdaRequest(
                    AllStreamId.Instance,
                    new CreateOsloSnapshotsSqsRequest
                    {
                        TicketId = ticketId,
                        Request = new CreateOsloSnapshotsRequest
                        {
                            PersistentLocalIds = [1]
                        },
                        ProvenanceData = Fixture.Create<ProvenanceData>()
                    }),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Complete(
                    ticketId,
                    new TicketResult("done"),
                    CancellationToken.None));
        }
    }
}
