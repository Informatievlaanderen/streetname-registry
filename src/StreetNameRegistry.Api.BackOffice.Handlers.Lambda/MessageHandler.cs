namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda
{
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using MediatR;
    using Requests;
    using Sqs.Requests;

    public sealed class MessageHandler : IMessageHandler
    {
        private readonly ILifetimeScope _container;

        public MessageHandler(ILifetimeScope container)
        {
            _container = container;
        }

        public async Task HandleMessage(object? messageData, MessageMetadata messageMetadata, CancellationToken cancellationToken)
        {
            messageMetadata.Logger?.LogInformation($"Handling message {messageData?.GetType().Name}");

            if (messageData is not SqsRequest sqsRequest)
            {
                messageMetadata.Logger?.LogInformation($"Unable to cast {nameof(messageData)} as {nameof(sqsRequest)}.");
                return;
            }

            await using var lifetimeScope = _container.BeginLifetimeScope();
            var mediator = lifetimeScope.Resolve<IMediator>();

            switch (sqsRequest)
            {
                case SqsStreetNameApproveRequest request:
                    await mediator.Send(new SqsLambdaStreetNameApproveRequest
                    {
                        Request = request.Request,
                        TicketId = request.TicketId,
                        MessageGroupId = messageMetadata.MessageGroupId!,
                        IfMatchHeaderValue = request.IfMatchHeaderValue,
                        Metadata = request.Metadata,
                        Provenance = request.ProvenanceData.ToProvenance()
                    }, cancellationToken);
                    break;

                case SqsStreetNameCorrectApprovalRequest request:
                    await mediator.Send(new SqsLambdaStreetNameCorrectApprovalRequest
                    {
                        Request = request.Request,
                        TicketId = request.TicketId,
                        MessageGroupId = messageMetadata.MessageGroupId!,
                        IfMatchHeaderValue = request.IfMatchHeaderValue,
                        Metadata = request.Metadata,
                        Provenance = request.ProvenanceData.ToProvenance()
                    }, cancellationToken);
                    break;

                case SqsStreetNameCorrectNamesRequest request:
                    await mediator.Send(new SqsLambdaStreetNameCorrectNamesRequest
                    {
                        Request = request.Request,
                        StreetNamePersistentLocalId = request.PersistentLocalId,
                        TicketId = request.TicketId,
                        MessageGroupId = messageMetadata.MessageGroupId!,
                        IfMatchHeaderValue = request.IfMatchHeaderValue,
                        Metadata = request.Metadata,
                        Provenance = request.ProvenanceData.ToProvenance()
                    }, cancellationToken);
                    break;

                case SqsStreetNameProposeRequest request:
                    await mediator.Send(new SqsLambdaStreetNameProposeRequest
                    {
                        Request = request.Request,
                        TicketId = request.TicketId,
                        MessageGroupId = messageMetadata.MessageGroupId!,
                        Metadata = request.Metadata,
                        Provenance = request.ProvenanceData.ToProvenance()
                    }, cancellationToken);
                    break;

                case SqsStreetNameRejectRequest request:
                    await mediator.Send(new SqsLambdaStreetNameRejectRequest
                    {
                        Request = request.Request,
                        TicketId = request.TicketId,
                        MessageGroupId = messageMetadata.MessageGroupId!,
                        IfMatchHeaderValue = request.IfMatchHeaderValue,
                        Metadata = request.Metadata,
                        Provenance = request.ProvenanceData.ToProvenance()
                    }, cancellationToken);
                    break;

                case SqsStreetNameCorrectRejectionRequest request:
                    await mediator.Send(new SqsLambdaStreetNameCorrectRejectionRequest
                    {
                        Request = request.Request,
                        TicketId = request.TicketId,
                        MessageGroupId = messageMetadata.MessageGroupId!,
                        IfMatchHeaderValue = request.IfMatchHeaderValue,
                        Metadata = request.Metadata,
                        Provenance = request.ProvenanceData.ToProvenance()
                    }, cancellationToken);
                    break;

                case SqsStreetNameRetireRequest request:
                    await mediator.Send(new SqsLambdaStreetNameRetireRequest
                    {
                        Request = request.Request,
                        TicketId = request.TicketId,
                        MessageGroupId = messageMetadata.MessageGroupId!,
                        IfMatchHeaderValue = request.IfMatchHeaderValue,
                        Metadata = request.Metadata,
                        Provenance = request.ProvenanceData.ToProvenance()
                    }, cancellationToken);
                    break;
            }
        }
    }
}
