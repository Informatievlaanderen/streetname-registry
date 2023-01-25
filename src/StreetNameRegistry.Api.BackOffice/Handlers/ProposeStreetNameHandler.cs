namespace StreetNameRegistry.Api.BackOffice.Handlers
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using Microsoft.EntityFrameworkCore;
    using Abstractions.Convertors;
    using Abstractions.SqsRequests;
    using Consumer;
    using TicketingService.Abstractions;

    public sealed class ProposeStreetNameHandler : SqsHandler<ProposeStreetNameSqsRequest>
    {
        private const string Action = "ProposeStreetName";
        private readonly ConsumerContext _consumerContext;

        public ProposeStreetNameHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            ConsumerContext consumerContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _consumerContext = consumerContext;
        }

        protected override string? WithAggregateId(ProposeStreetNameSqsRequest request)
        {
            var identifier = request.Request.GemeenteId
                .AsIdentifier()
                .Map(IdentifierMappings.MunicipalityNisCode);

            var municipality = _consumerContext.MunicipalityConsumerItems
                .AsNoTracking()
                .SingleOrDefault(item => item.NisCode == identifier.Value);

            return municipality?.MunicipalityId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, ProposeStreetNameSqsRequest sqsRequest)
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
