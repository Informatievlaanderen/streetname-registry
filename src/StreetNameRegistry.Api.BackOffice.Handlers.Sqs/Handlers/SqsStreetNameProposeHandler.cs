namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Handlers
{
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions.Convertors;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using Consumer;
    using Microsoft.EntityFrameworkCore;
    using Requests;
    using TicketingService.Abstractions;

    public sealed class SqsStreetNameProposeHandler : SqsHandler<SqsStreetNameProposeRequest>
    {
        private const string Action = "ProposeStreetName";
        private readonly ConsumerContext _consumerContext;

        public SqsStreetNameProposeHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            ConsumerContext consumerContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _consumerContext = consumerContext;
        }

        protected override string? WithAggregateId(SqsStreetNameProposeRequest request)
        {
            var identifier = request.Request.GemeenteId
                .AsIdentifier()
                .Map(IdentifierMappings.MunicipalityNisCode);

            var municipality = _consumerContext.MunicipalityConsumerItems
                .AsNoTracking()
                .SingleOrDefault(item => item.NisCode == identifier.Value);

            return municipality?.MunicipalityId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, SqsStreetNameProposeRequest sqsRequest)
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
