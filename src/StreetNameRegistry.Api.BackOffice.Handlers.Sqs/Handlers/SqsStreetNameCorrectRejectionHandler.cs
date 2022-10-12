namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Handlers
{
    using System.Collections.Generic;
    using Abstractions;
    using Requests;
    using TicketingService.Abstractions;

    public sealed class SqsStreetNameCorrectRejectionHandler : SqsHandler<SqsStreetNameCorrectRejectionRequest>
    {
        public const string Action = "CorrectStreetNameRejection";

        private readonly BackOfficeContext _backOfficeContext;

        public SqsStreetNameCorrectRejectionHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base (sqsQueue, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string? WithAggregateId(SqsStreetNameCorrectRejectionRequest request)
        {
            var municipalityIdByPersistentLocalId = _backOfficeContext
                .MunicipalityIdByPersistentLocalId
                .Find(request.Request.PersistentLocalId);

            return municipalityIdByPersistentLocalId?.MunicipalityId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, SqsStreetNameCorrectRejectionRequest sqsRequest)
        {
            return new Dictionary<string, string>
            {
                { RegistryKey, nameof(StreetNameRegistry) },
                { ActionKey, Action },
                { AggregateIdKey, aggregateId },
                { ObjectIdKey, sqsRequest.Request.PersistentLocalId.ToString() }
            };
        }
    }
}
