namespace StreetNameRegistry.Api.Oslo.StreetName.Count
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using List;
    using MediatR;
    using Projections.Legacy;
    using Query;

    public sealed record OsloCountRequest(FilteringHeader<StreetNameFilter> Filtering, SortingHeader Sorting) : IRequest<TotaalAantalResponse>;

    public sealed class OsloCountHandlerV2 : IRequestHandler<OsloCountRequest, TotaalAantalResponse>
    {
        private readonly LegacyContext _legacyContext;

        public OsloCountHandlerV2(LegacyContext legacyContext)
        {
            _legacyContext = legacyContext;
        }

        public async Task<TotaalAantalResponse> Handle(OsloCountRequest request, CancellationToken cancellationToken)
        {
            return
                new TotaalAantalResponse
                {
                    Aantal = await new StreetNameListOsloQueryV2(_legacyContext)
                        .CountAsync(request.Filtering, cancellationToken)
                };
        }
    }
}
