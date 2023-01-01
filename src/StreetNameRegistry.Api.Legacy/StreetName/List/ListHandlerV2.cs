namespace StreetNameRegistry.Api.Legacy.StreetName.List
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Convertors;
    using global::Microsoft.EntityFrameworkCore;
    using global::Microsoft.Extensions.Options;
    using Infrastructure.Options;
    using Municipality;
    using Projections.Legacy;
    using Projections.Legacy.StreetNameListV2;
    using Projections.Syndication;
    using Query;

    public sealed class ListHandlerV2 : ListHandlerBase
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public ListHandlerV2(
            LegacyContext legacyContext,
            SyndicationContext syndicationContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
            _responseOptions = responseOptions;
        }

        public override async Task<StreetNameListResponse> Handle(ListRequest request, CancellationToken cancellationToken)
        {
            var pagedStreetNamesV2 = new StreetNameListQueryV2(_legacyContext, _syndicationContext)
                    .Fetch<StreetNameListItemV2, StreetNameListItemV2>(request.Filtering, request.Sorting, request.Pagination);

            return
                new StreetNameListResponse
                {
                    Straatnamen = await pagedStreetNamesV2
                        .Items
                        .Select(m => new StreetNameListItemResponse(
                            m.PersistentLocalId,
                            _responseOptions.Value.Naamruimte,
                            _responseOptions.Value.DetailUrl,
                            GetGeografischeNaamByTaal(m, m.PrimaryLanguage),
                            GetHomoniemToevoegingByTaal(m, m.PrimaryLanguage),
                            m.Status.ConvertFromMunicipalityStreetNameStatus(),
                            m.VersionTimestamp.ToBelgianDateTimeOffset()))
                        .ToListAsync(cancellationToken),
                    Volgende = BuildNextUri(pagedStreetNamesV2.PaginationInfo, _responseOptions.Value.VolgendeUrl),
                    Pagination = pagedStreetNamesV2.PaginationInfo,
                    Sorting = pagedStreetNamesV2.Sorting
                };
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
