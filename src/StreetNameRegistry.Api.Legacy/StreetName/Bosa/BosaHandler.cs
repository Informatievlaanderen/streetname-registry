namespace StreetNameRegistry.Api.Legacy.StreetName.Bosa
{
    using System.Threading;
    using System.Threading.Tasks;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;
    using Query;

    public sealed class BosaHandler : IRequestHandler<BosaStreetNameRequest, StreetNameBosaResponse>
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public BosaHandler(
            LegacyContext legacyContext,
            SyndicationContext syndicationContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
            _responseOptions = responseOptions;
        }

        public async Task<StreetNameBosaResponse> Handle(BosaStreetNameRequest request, CancellationToken cancellationToken)
        {
            var filter = new StreetNameNameFilter(request);

            return await
                new StreetNameBosaQuery(
                        _legacyContext,
                        _syndicationContext,
                        _responseOptions)
                    .FilterAsync(filter, cancellationToken);
        }
    }
}
