namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Lambda
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using MediatR;
    using Requests;

    public class MessageHandler : IMessageHandler
    {
        private readonly IMediator _mediator;

        public MessageHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task HandleMessage(object? messageData, MessageMetadata messageMetadata, CancellationToken cancellationToken)
        {
            messageMetadata.Logger?.LogInformation($"Handling message {messageData?.GetType().Name}");

            var sqsLambdaRequest = messageData as SqsLambdaRequest;

            if (sqsLambdaRequest is null)
            {
                throw new InvalidOperationException($"Unable to cast {nameof(messageData)} as {nameof(sqsLambdaRequest)}.");
            }

            sqsLambdaRequest.MessageGroupId = messageMetadata.MessageGroupId;

            // TODO: uncomment after initial lambda testing
            //switch (sqsLambdaRequest)
            //{
            //    // Building

            //    case SqsPlanBuildingRequest sqsPlanBuildingRequest:
            //        await _mediator.Send(sqsPlanBuildingRequest, cancellationToken);
            //        break;

            //    case SqsPlaceBuildingUnderConstructionRequest sqsPlaceBuildingUnderConstructionRequest:
            //        await _mediator.Send(sqsPlaceBuildingUnderConstructionRequest, cancellationToken);
            //        break;

            //    case SqsRealizeBuildingRequest sqsRealizeBuildingRequest:
            //        await _mediator.Send(sqsRealizeBuildingRequest, cancellationToken);
            //        break;

            //    // BuildingUnit

            //    case SqsPlanBuildingUnitRequest sqsPlanBuildingUnitRequest:
            //        await _mediator.Send(sqsPlanBuildingUnitRequest, cancellationToken);
            //        break;
            //}
        }
    }
}
