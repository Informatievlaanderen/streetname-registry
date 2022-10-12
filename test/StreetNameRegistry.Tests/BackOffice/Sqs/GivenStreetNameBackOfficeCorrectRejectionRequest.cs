namespace StreetNameRegistry.Tests.BackOffice.Sqs
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using FluentAssertions;
    using global::AutoFixture;
    using Moq;
    using Municipality;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Exceptions;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Handlers.Sqs;
    using StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Handlers;
    using StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests;
    using Testing;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenStreetNameBackOfficeCorrectRejectionRequest : StreetNameRegistryTest
    {
        private readonly TestBackOfficeContext _backOfficeContext;

        public GivenStreetNameBackOfficeCorrectRejectionRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedPersistentLocalId());

            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
        }

        [Fact]
        public async Task ThenTicketWithLocationIsCreated()
        {
            // Arrange
            var municipalityLatestItem = _backOfficeContext.AddMunicipalityIdByPersistentLocalIdToFixture(
                Fixture.Create<PersistentLocalId>(),
                Fixture.Create<MunicipalityId>());

            var ticketId = Fixture.Create<Guid>();
            var ticketingMock = new Mock<ITicketing>();
            ticketingMock
                .Setup(x => x.CreateTicket(It.IsAny<IDictionary<string, string>>(), CancellationToken.None))
                .ReturnsAsync(ticketId);

            var ticketingUrl = new TicketingUrl(Fixture.Create<Uri>().ToString());

            var sqsQueue = new Mock<ISqsQueue>();

            var sut = new SqsStreetNameCorrectRejectionHandler(
                sqsQueue.Object,
                ticketingMock.Object,
                ticketingUrl,
                _backOfficeContext);

            var sqsRequest = new SqsStreetNameCorrectRejectionRequest
            {
                Request = new StreetNameBackOfficeCorrectRejectionRequest
                {
                    PersistentLocalId = Fixture.Create<PersistentLocalId>()
                }
            };

            // Act
            var result = await sut.Handle(sqsRequest, CancellationToken.None);

            // Assert
            sqsRequest.TicketId.Should().Be(ticketId);
            sqsQueue.Verify(x => x.Copy(
                sqsRequest,
                It.Is<SqsQueueOptions>(y => y.MessageGroupId == municipalityLatestItem.MunicipalityId.ToString("D")),
                CancellationToken.None));
            result.Location.Should().Be(ticketingUrl.For(ticketId));
        }

        [Fact]
        public void WithNoMunicipalityFoundByStreetNamePersistentLocalId_ThrowsAggregateIdNotFound()
        {
            // Arrange
            var sut = new SqsStreetNameCorrectRejectionHandler(
                Mock.Of<ISqsQueue>(),
                Mock.Of<ITicketing>(),
                Mock.Of<ITicketingUrl>(),
                _backOfficeContext);

            // Act
            var act = async () => await sut.Handle(
                new SqsStreetNameCorrectRejectionRequest
                {
                    Request = Fixture.Create<StreetNameBackOfficeCorrectRejectionRequest>()
                }, CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<AggregateIdIsNotFoundException>();
        }
    }
}
