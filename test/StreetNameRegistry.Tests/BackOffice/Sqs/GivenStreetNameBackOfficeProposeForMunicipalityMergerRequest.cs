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
    using StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using StreetNameRegistry.Api.BackOffice.Handlers;
    using Testing;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenStreetNameBackOfficeProposeForMunicipalityMergerRequest : StreetNameRegistryTest
    {
        private readonly TestConsumerContext _testConsumerContext;

        public GivenStreetNameBackOfficeProposeForMunicipalityMergerRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _testConsumerContext = new FakeConsumerContextFactory().CreateDbContext();
        }

        [Fact]
        public async Task ThenTicketWithLocationIsCreated()
        {
            // Arrange
            var nisCode = "23002";
            var municipalityLatestItem = _testConsumerContext.AddMunicipalityLatestItemFixtureWithNisCode(nisCode);

            var sqsRequest = Fixture.Create<ProposeStreetNamesForMunicipalityMergerSqsRequest>();
            sqsRequest.NisCode = nisCode;

            var ticketId = Fixture.Create<Guid>();
            var ticketingMock = new Mock<ITicketing>();
            ticketingMock
                .Setup(x => x.CreateTicket(It.IsAny<IDictionary<string, string>>(), CancellationToken.None))
                .ReturnsAsync(ticketId);

            var ticketingUrl = new TicketingUrl(Fixture.Create<Uri>().ToString());

            var sqsQueue = new Mock<ISqsQueue>();

            var sut = new ProposeStreetNamesForMunicipalityMergerHandler(
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
            var sut = new ProposeStreetNamesForMunicipalityMergerHandler(
                Mock.Of<ISqsQueue>(),
                Mock.Of<ITicketing>(),
                Mock.Of<ITicketingUrl>(),
                _testConsumerContext);

            // Act
            var act = async () => await sut.Handle(
                Fixture.Create<ProposeStreetNamesForMunicipalityMergerSqsRequest>(),
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<AggregateIdIsNotFoundException>();
        }
    }
}
