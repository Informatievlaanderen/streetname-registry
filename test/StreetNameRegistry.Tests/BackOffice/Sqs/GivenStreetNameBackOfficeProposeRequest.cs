namespace StreetNameRegistry.Tests.BackOffice.Sqs
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using FluentAssertions;
    using global::AutoFixture;
    using Moq;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Handlers;
    using StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests;
    using Testing;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenStreetNameBackOfficeProposeRequest : StreetNameRegistryTest
    {
        private readonly TestConsumerContext _testConsumerContext;

        public GivenStreetNameBackOfficeProposeRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _testConsumerContext = new FakeConsumerContextFactory().CreateDbContext();
        }

        [Fact]
        public async Task ThenTicketWithLocationIsCreated()
        {
            // Arrange
            var municipalityLatestItem = _testConsumerContext.AddMunicipalityLatestItemFixtureWithNisCode("23002");

            var request = Fixture.Create<ProposeStreetNameBackOfficeRequest>();
            request.GemeenteId = $"https://data.vlaanderen.be/id/gemeente/{municipalityLatestItem.NisCode}";

            var sqsRequest = new ProposeStreetNameSqsRequest { Request = request };

            var ticketId = Fixture.Create<Guid>();
            var ticketingMock = new Mock<ITicketing>();
            ticketingMock
                .Setup(x => x.CreateTicket(It.IsAny<IDictionary<string, string>>(), CancellationToken.None))
                .ReturnsAsync(ticketId);

            var ticketingUrl = new TicketingUrl(Fixture.Create<Uri>().ToString());

            var sqsQueue = new Mock<ISqsQueue>();

            var sut = new ProposeStreetNameSqsHandler(
                sqsQueue.Object,
                ticketingMock.Object,
                ticketingUrl,
                _testConsumerContext);

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
        public void ForNotExistingMunicipality_ThrowsAggregateIdNotFound()
        {
            // Arrange
            var sut = new ProposeStreetNameSqsHandler(
                Mock.Of<ISqsQueue>(),
                Mock.Of<ITicketing>(),
                Mock.Of<ITicketingUrl>(),
                _testConsumerContext);

            // Act
            var act = async () => await sut.Handle(
                new ProposeStreetNameSqsRequest { Request = Fixture.Create<ProposeStreetNameBackOfficeRequest>() },
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<AggregateIdIsNotFoundException>();
        }
    }
}
