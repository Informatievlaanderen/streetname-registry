namespace StreetNameRegistry.Api.Legacy.StreetName.Bosa
{
    using System.Threading;
    using System.Threading.Tasks;
    using global::Microsoft.Extensions.Options;
    using Infrastructure.Options;
    using MediatR;
    using Projections.Legacy;
    using Projections.Syndication;
    using Query;

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
