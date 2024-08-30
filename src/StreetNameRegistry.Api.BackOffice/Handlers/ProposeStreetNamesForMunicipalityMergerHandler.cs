namespace StreetNameRegistry.Api.BackOffice.Handlers
{
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using Consumer;
    using Microsoft.EntityFrameworkCore;
    using TicketingService.Abstractions;

    public class ProposeStreetNamesForMunicipalityMergerHandler : SqsHandler<ProposeStreetNamesForMunicipalityMergerSqsRequest>
    {
        private const string Action = "ProposeStreetNamesForMunicipalityMerger";
        private readonly ConsumerContext _consumerContext;

        public ProposeStreetNamesForMunicipalityMergerHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            ConsumerContext consumerContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _consumerContext = consumerContext;
        }

        protected override string? WithAggregateId(ProposeStreetNamesForMunicipalityMergerSqsRequest request)
        {
            var municipality = _consumerContext.MunicipalityConsumerItems
                .AsNoTracking()
                .SingleOrDefault(item => item.NisCode == request.NisCode);

            return municipality?.MunicipalityId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, ProposeStreetNamesForMunicipalityMergerSqsRequest sqsRequest)
        {
            return new Dictionary<string, string>
            {
                { RegistryKey, nameof(StreetNameRegistry) },
                { ActionKey, Action },
                { AggregateIdKey, aggregateId }
            };
        }
    }
}
