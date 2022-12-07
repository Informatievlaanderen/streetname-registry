namespace StreetNameRegistry.Api.Oslo.StreetName.List
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Converters;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Municipality;
    using Projections.Legacy.StreetNameListV2;
    using Query;

    public class OsloListHandlerV2 : OsloListHandlerBase
    {
        public override async Task<IActionResult> Handle(OsloListRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.HttpRequest.ExtractFilteringRequest<StreetNameFilter>();
            var sorting = request.HttpRequest.ExtractSortingRequest();
            var pagination = request.HttpRequest.ExtractPaginationRequest();

            var pagedStreetNamesV2 = new StreetNameListOsloQueryV2(request.LegacyContext, request.SyndicationContext)
                    .Fetch<StreetNameListItemV2, StreetNameListItemV2>(filtering, sorting, pagination);

            request.HttpResponse.AddPagedQueryResultHeaders(pagedStreetNamesV2);

            return new OkObjectResult(
                new StreetNameListOsloResponse
                {
                    Straatnamen = await pagedStreetNamesV2
                        .Items
                        .Select(m => new StreetNameListOsloItemResponse(
                            m.PersistentLocalId,
                            request.ResponseOptions.Value.Naamruimte,
                            request.ResponseOptions.Value.DetailUrl,
                            GetGeografischeNaamByTaal(m, m.PrimaryLanguage),
                            GetHomoniemToevoegingByTaal(m, m.PrimaryLanguage),
                            m.Status.ConvertFromMunicipalityStreetNameStatus(),
                            m.VersionTimestamp.ToBelgianDateTimeOffset()))
                        .ToListAsync(cancellationToken),
                    Volgende = BuildNextUri(pagedStreetNamesV2.PaginationInfo, request.ResponseOptions.Value.VolgendeUrl),
                    Context = request.ResponseOptions.Value.ContextUrlList
                });
        }

        private static GeografischeNaam GetGeografischeNaamByTaal(StreetNameListItemV2 item, Language? taal)
        {
            switch (taal)
            {
                case null when !string.IsNullOrEmpty(item.NameDutch):
                case Language.Dutch when !string.IsNullOrEmpty(item.NameDutch):
                    return new GeografischeNaam(
                        item.NameDutch,
                        Taal.NL);

                case Language.French when !string.IsNullOrEmpty(item.NameFrench):
                    return new GeografischeNaam(
                        item.NameFrench,
                        Taal.FR);

                case Language.German when !string.IsNullOrEmpty(item.NameGerman):
                    return new GeografischeNaam(
                        item.NameGerman,
                        Taal.DE);

                case Language.English when !string.IsNullOrEmpty(item.NameEnglish):
                    return new GeografischeNaam(
                        item.NameEnglish,
                        Taal.EN);

                default:
                    return null;
            }
        }

        private static GeografischeNaam? GetHomoniemToevoegingByTaal(StreetNameListItemV2 item, Language? taal)
        {
            switch (taal)
            {
                case null when !string.IsNullOrEmpty(item.HomonymAdditionDutch):
                case Language.Dutch when !string.IsNullOrEmpty(item.HomonymAdditionDutch):
                    return new GeografischeNaam(
                        item.HomonymAdditionDutch,
                        Taal.NL);

                case Language.French when !string.IsNullOrEmpty(item.HomonymAdditionFrench):
                    return new GeografischeNaam(
                        item.HomonymAdditionFrench,
                        Taal.FR);

                case Language.German when !string.IsNullOrEmpty(item.HomonymAdditionGerman):
                    return new GeografischeNaam(
                        item.HomonymAdditionGerman,
                        Taal.DE);

                case Language.English when !string.IsNullOrEmpty(item.HomonymAdditionEnglish):
                    return new GeografischeNaam(
                        item.HomonymAdditionEnglish,
                        Taal.EN);

                default:
                    return null;
            }
        }
    }
}
