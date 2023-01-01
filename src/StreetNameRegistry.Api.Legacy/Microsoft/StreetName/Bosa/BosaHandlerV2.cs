namespace StreetNameRegistry.Api.Legacy.Microsoft.StreetName.Bosa
{
    using System.Threading;
    using System.Threading.Tasks;
    using global::Microsoft.Extensions.Options;
    using MediatR;
    using Query;
    using Infrastructure.Options;
    using StreetNameRegistry.Projections.Legacy.Microsoft;
    using StreetNameRegistry.Projections.Syndication.Microsoft;

    public sealed class BosaHandlerV2 : IRequestHandler<BosaStreetNameRequest, StreetNameBosaResponse>
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public BosaHandlerV2(
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
            var filter = new StreetNameNameFilterV2(request);

            return await
                new StreetNameBosaQueryV2(
                        _legacyContext,
                        _syndicationContext,
                        _responseOptions)
                    .FilterAsync(filter, cancellationToken);
        }
    }
}
