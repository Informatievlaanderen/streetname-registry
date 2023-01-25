namespace StreetNameRegistry.Api.BackOffice.Handlers
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using Abstractions;
    using Abstractions.SqsRequests;
    using TicketingService.Abstractions;

    public sealed class RetireStreetNameHandler : SqsHandler<RetireStreetNameSqsRequest>
    {
        public const string Action = "RetireStreetName";

        private readonly BackOfficeContext _backOfficeContext;

        public RetireStreetNameHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base (sqsQueue, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string? WithAggregateId(RetireStreetNameSqsRequest request)
        {
            var municipalityIdByPersistentLocalId = _backOfficeContext
                .MunicipalityIdByPersistentLocalId
                .Find(request.Request.PersistentLocalId);

            return municipalityIdByPersistentLocalId?.MunicipalityId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, RetireStreetNameSqsRequest sqsRequest)
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
