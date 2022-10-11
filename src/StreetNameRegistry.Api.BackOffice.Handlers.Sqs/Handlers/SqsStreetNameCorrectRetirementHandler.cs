namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Handlers
{
    using System.Collections.Generic;
    using Abstractions;
    using Requests;
    using TicketingService.Abstractions;

    public sealed class SqsStreetNameCorrectRetirementHandler : SqsHandler<SqsStreetNameCorrectRetirementRequest>
    {
        public const string Action = "CorrectStreetNameRetirement";

        private readonly BackOfficeContext _backOfficeContext;

        public SqsStreetNameCorrectRetirementHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base (sqsQueue, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string? WithAggregateId(SqsStreetNameCorrectRetirementRequest request)
        {
            var municipalityIdByPersistentLocalId = _backOfficeContext
                .MunicipalityIdByPersistentLocalId
                .Find(request.Request.PersistentLocalId);

            return municipalityIdByPersistentLocalId?.MunicipalityId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, SqsStreetNameCorrectRetirementRequest sqsRequest)
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
