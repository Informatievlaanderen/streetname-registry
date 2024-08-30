namespace StreetNameRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentAssertions;
    using global::AutoFixture;
    using MediatR;
    using Moq;
    using StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests;
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
                It.IsAny<CancellationToken>());

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
                It.IsAny<CancellationToken>());

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

            var messageData = Fixture.Create<ProposeStreetNameSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<ProposeStreetNameLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == null &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingSqsStreetNameApproveRequest_ThenSqsLambdaStreetNameApproveRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<ApproveStreetNameSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<ApproveStreetNameLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingSqsStreetNameCorrectApprovalRequest_ThenSqsLambdaStreetNameCorrectApprovalRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CorrectStreetNameApprovalSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectStreetNameApprovalLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingSqsStreetNameCorrectNamesRequest_ThenSqsLambdaStreetNameCorrectNamesRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CorrectStreetNameNamesSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectStreetNameNamesLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata &&
                    request.StreetNamePersistentLocalId == messageData.PersistentLocalId
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingSqsStreetNameChangeNamesRequest_ThenSqsLambdaStreetNameChangeNamesRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<ChangeStreetNameNamesSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<ChangeStreetNameNamesLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata &&
                    request.StreetNamePersistentLocalId == messageData.PersistentLocalId
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingSqsStreetNameRejectRequest_ThenSqsLambdaStreetNameRejectRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<RejectStreetNameSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RejectStreetNameLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingSqsStreetNameCorrectRejectionRequest_ThenSqsLambdaStreetNameCorrectRejectionRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CorrectStreetNameRejectionSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectStreetNameRejectionLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingSqsStreetNameRetireRequest_ThenSqsLambdaStreetNameRetireRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<RetireStreetNameSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RetireStreetNameLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingSqsStreetNameRenameRequest_ThenSqsLambdaStreetNameRenameRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<RenameStreetNameSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RenameStreetNameLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.StreetNamePersistentLocalId == messageData.PersistentLocalId &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingSqsStreetNameCorrectRetirementRequest_ThenSqsLambdaStreetNameCorrectRetirementRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CorrectStreetNameRetirementSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectStreetNameRetirementLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingSqsStreetNameCorrectStreetNameHomonymAdditionsRequest_ThenSqsLambdaStreetNameCorrectStreetNameHomonymAdditionsRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CorrectStreetNameHomonymAdditionsSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectStreetNameHomonymAdditionsLambdaRequest>(request =>
                    request.StreetNamePersistentLocalId == messageData.PersistentLocalId &&
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingSqsStreetNameRemoveRequest_ThenSqsLambdaStreetNameRemoveRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<RemoveStreetNameSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RemoveStreetNameLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProposeStreetNamesForMunicipalityMergerRequest_ThenSqsLambdaProposeStreetNamesForMunicipalityMergerRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<ProposeStreetNamesForMunicipalityMergerSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<ProposeStreetNamesForMunicipalityMergerLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.NisCode == messageData.NisCode &&
                    request.StreetNames == messageData.StreetNames &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    internal class TestSqsRequest : SqsRequest
    { }
}
