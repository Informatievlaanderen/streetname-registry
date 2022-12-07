namespace StreetNameRegistry.Api.Oslo.StreetName.Count
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using MediatR;
    using Query;

    public sealed record OsloCountRequest(FilteringHeader<StreetNameFilter> Filtering, SortingHeader Sorting) : IRequest<TotaalAantalResponse>;

    public abstract class OsloCountHandlerBase : IRequestHandler<OsloCountRequest, TotaalAantalResponse>
    {
        public abstract Task<TotaalAantalResponse> Handle(OsloCountRequest request, CancellationToken cancellationToken);
    }
}
