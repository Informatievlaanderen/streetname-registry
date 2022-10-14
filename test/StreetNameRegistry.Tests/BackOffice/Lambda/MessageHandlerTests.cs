namespace StreetNameRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using FluentAssertions;
    using global::AutoFixture;
    using MediatR;
    using Moq;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class MessageHandlerTests : StreetNameRegistryTest
    {
        public MessageHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public async Task WhenProcessingUnknownMessage_ThenNothingIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<object>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task WhenProcessingSqsRequestWithoutCorrespondingSqsLambdaRequest_ThenThrowsNotImplementedException()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<TestSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            var act = async () => await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Fact]
        public async Task WhenProcessingSqsStreetNameProposeRequest_ThenSqsLambdaStreetNameProposeRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<SqsStreetNameProposeRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<SqsLambdaStreetNameProposeRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == null &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingSqsStreetNameApproveRequest_ThenSqsLambdaStreetNameApproveRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<SqsStreetNameApproveRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<SqsLambdaStreetNameApproveRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingSqsStreetNameCorrectApprovalRequest_ThenSqsLambdaStreetNameCorrectApprovalRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<SqsStreetNameCorrectApprovalRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<SqsLambdaStreetNameCorrectApprovalRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingSqsStreetNameCorrectNamesRequest_ThenSqsLambdaStreetNameCorrectNamesRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<SqsStreetNameCorrectNamesRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<SqsLambdaStreetNameCorrectNamesRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata &&
                    request.StreetNamePersistentLocalId == messageData.PersistentLocalId
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingSqsStreetNameRejectRequest_ThenSqsLambdaStreetNameRejectRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<SqsStreetNameRejectRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<SqsLambdaStreetNameRejectRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingSqsStreetNameCorrectRejectionRequest_ThenSqsLambdaStreetNameCorrectRejectionRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<SqsStreetNameCorrectRejectionRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<SqsLambdaStreetNameCorrectRejectionRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingSqsStreetNameRetireRequest_ThenSqsLambdaStreetNameRetireRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<SqsStreetNameRetireRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<SqsLambdaStreetNameRetireRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingSqsStreetNameCorrectRetirementRequest_ThenSqsLambdaStreetNameCorrectRetirementRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<SqsStreetNameCorrectRetirementRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<SqsLambdaStreetNameCorrectRetirementRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }
    }

    public class TestSqsRequest : SqsRequest
    { }
}
