namespace StreetNameRegistry.Api.Legacy.StreetName.Count
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using MediatR;
    using Query;

    public sealed record CountRequest(FilteringHeader<StreetNameFilter> Filtering, SortingHeader Sorting) : IRequest<TotaalAantalResponse>;

    public abstract class CountHandlerBase : IRequestHandler<CountRequest, TotaalAantalResponse>
    {
        public abstract Task<TotaalAantalResponse> Handle(CountRequest request, CancellationToken cancellationToken);
    }
}
