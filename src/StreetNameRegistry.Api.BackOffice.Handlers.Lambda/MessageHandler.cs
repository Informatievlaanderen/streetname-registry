namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda
{
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
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
                case ApproveStreetNameSqsRequest request:
                    await mediator.Send(new ApproveStreetNameLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case CorrectStreetNameApprovalSqsRequest request:
                    await mediator.Send(new CorrectStreetNameApprovalLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case CorrectStreetNameNamesSqsRequest request:
                    await mediator.Send(new CorrectStreetNameNamesLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case ProposeStreetNameSqsRequest request:
                    await mediator.Send(new ProposeStreetNameLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case RejectStreetNameSqsRequest request:
                    await mediator.Send(new RejectStreetNameLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case CorrectStreetNameRejectionSqsRequest request:
                    await mediator.Send(new CorrectStreetNameRejectionLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case RetireStreetNameSqsRequest request:
                    await mediator.Send(new RetireStreetNameLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case CorrectStreetNameRetirementSqsRequest request:
                    await mediator.Send(new CorrectStreetNameRetirementLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                default:
                    throw new NotImplementedException(
                        $"{sqsRequest.GetType().Name} has no corresponding SqsLambdaRequest defined.");
            }
        }
    }
}
