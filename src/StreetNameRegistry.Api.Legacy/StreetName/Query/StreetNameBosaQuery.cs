namespace StreetNameRegistry.Api.Legacy.StreetName.Query
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Bosa;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Bosa;
    using Convertors;
    using Infrastructure.Options;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;
    using Projections.Syndication.Municipality;
    using StreetNameRegistry.StreetName;
    using StreetNameName = Projections.Legacy.StreetNameName.StreetNameName;

    public sealed class StreetNameBosaQuery
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptions> _responseOptionsProvider;

        public StreetNameBosaQuery(
            LegacyContext legacyContext,
            SyndicationContext syndicationContext,
            IOptions<ResponseOptions> responseOptionsProvider)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
            _responseOptionsProvider = responseOptionsProvider;
        }

        public async Task<StreetNameBosaResponse> FilterAsync(StreetNameNameFilter filter, CancellationToken ct = default)
        {
            var shouldFilterOnMunicipalityObjectId = !string.IsNullOrEmpty(filter.MunicipalityObjectId);
            var shouldFilterOnMunicipalityVersion =  !string.IsNullOrEmpty(filter.MunicipalityVersion);

            var municipalitiesQueryable = _syndicationContext
                .MunicipalityLatestItems
                .AsNoTracking();

            if (shouldFilterOnMunicipalityObjectId)
                municipalitiesQueryable = municipalitiesQueryable.Where(m => m.NisCode == filter.MunicipalityObjectId);

            if (shouldFilterOnMunicipalityVersion)
                municipalitiesQueryable = municipalitiesQueryable.Where(m => m.Version == filter.MunicipalityVersion);

            var streetNames = _legacyContext
                .StreetNameNames
                .AsNoTracking()
                .Where(s => !s.Removed && s.IsFlemishRegion && s.Complete);

            var municipalities = municipalitiesQueryable.OrderByDescending(m => m.Version).ToList();

            if (shouldFilterOnMunicipalityObjectId || shouldFilterOnMunicipalityVersion)
            {
                if (!municipalities.Any())
                    return new StreetNameBosaResponse();

                var nisCodes = municipalities.Select(m => m.NisCode).Distinct();
                streetNames = streetNames.Where(s => nisCodes.Contains(s.NisCode));
            }

            if (!string.IsNullOrEmpty(filter.ObjectId))
            {
                if (!int.TryParse(filter.ObjectId, out var persistentLocalId))
                    return new StreetNameBosaResponse();

                streetNames = streetNames.Where(s => s.PersistentLocalId == persistentLocalId);
            }

            if (filter.StreetNameVersion.HasValue)
                streetNames = streetNames.Where(m => m.VersionTimestampAsDateTimeOffset == filter.StreetNameVersion.Value);

            if (filter.Status.HasValue)
                streetNames = streetNames.Where(s => s.Status == filter.Status.Value);

            if (!string.IsNullOrEmpty(filter.StreetName))
                streetNames = CompareByCompareType(
                    streetNames,
                    filter.StreetName,
                    filter.Language,
                    filter.IsContainsFilter);
            else if (filter.Language.HasValue)
                streetNames = ApplyLanguageFilter(streetNames, filter.Language.Value);

            var names = await TransformAsync(streetNames, municipalities, filter.Language, ct);

            return new StreetNameBosaResponse
            {
                Straatnamen = names.ToList(),
            };
        }

        private async Task<IEnumerable<StreetNameBosaItemResponse>> TransformAsync(
            IQueryable<StreetNameName> streetNamesQueryable,
            IReadOnlyCollection<MunicipalityLatestItem> municipalities,
            Language? language,
            CancellationToken ct)
        {
            var streetNameResponse = new List<StreetNameBosaItemResponse>();
            var streetNames = await streetNamesQueryable
                .OrderBy(s => s.PersistentLocalId)
                .Take(1001)
                .ToListAsync(ct);

            // BOSA endpoints return max 1001 elements.
            // The OrderBy clause ensures predictable results.
            foreach (var streetName in streetNames)
            {
                var municipality = municipalities.FirstOrDefault(m => m.NisCode == streetName.NisCode);

                streetNameResponse.Add(new StreetNameBosaItemResponse(
                    streetName.PersistentLocalId.ToString(),
                    streetName.NisCode,
                    _responseOptionsProvider.Value.Naamruimte,
                    _responseOptionsProvider.Value.GemeenteNaamruimte,
                    streetName.VersionTimestamp.ToBelgianDateTimeOffset(),
                    municipality?.Version,
                    streetName.Status.ConvertFromStreetNameStatus(),
                    GetStreetNamesByLanguage(streetName, language),
                    GetMunicipalityNames(municipality)));
            }

            return streetNameResponse;
        }

        private static IEnumerable<GeografischeNaam> GetMunicipalityNames(MunicipalityLatestItem municipality)
        {
            var municipalityNames = new List<GeografischeNaam>();

            if (municipality == null)
                return municipalityNames;

            if (!string.IsNullOrEmpty(municipality.NameDutch))
                municipalityNames.Add(new GeografischeNaam(municipality.NameDutch, Taal.NL));

            if (!string.IsNullOrEmpty(municipality.NameFrench))
                municipalityNames.Add(new GeografischeNaam(municipality.NameFrench, Taal.FR));

            if (!string.IsNullOrEmpty(municipality.NameGerman))
                municipalityNames.Add(new GeografischeNaam(municipality.NameGerman, Taal.DE));

            if (!string.IsNullOrEmpty(municipality.NameEnglish))
                municipalityNames.Add(new GeografischeNaam(municipality.NameEnglish, Taal.EN));

            return municipalityNames;
        }

        private static IEnumerable<GeografischeNaam> GetStreetNamesByLanguage(StreetNameName streetName, Language? language)
        {
            var streetNames = new List<GeografischeNaam>();

            if (language.HasValue)
            {
                streetNames.Add(new GeografischeNaam(streetName.GetNameValueByLanguage(language.Value), (Taal)language.Value));
                return streetNames;
            }

            if (!string.IsNullOrEmpty(streetName.NameDutch))
                streetNames.Add(new GeografischeNaam(streetName.NameDutch, Taal.NL));

            if (!string.IsNullOrEmpty(streetName.NameFrench))
                streetNames.Add(new GeografischeNaam(streetName.NameFrench, Taal.FR));

            if (!string.IsNullOrEmpty(streetName.NameGerman))
                streetNames.Add(new GeografischeNaam(streetName.NameGerman, Taal.DE));

            if (!string.IsNullOrEmpty(streetName.NameEnglish))
                streetNames.Add(new GeografischeNaam(streetName.NameEnglish, Taal.EN));

            return streetNames;
        }

        private static IQueryable<StreetNameName> ApplyLanguageFilter(IQueryable<StreetNameName> query, Language language)
        {
            switch (language)
            {
                default:
                case Language.Dutch:
                    return query.Where(m => m.NameDutchSearch != null);

                case Language.French:
                    return query.Where(m => m.NameFrenchSearch != null);

                case Language.German:
                    return query.Where(m => m.NameGermanSearch != null);

                case Language.English:
                    return query.Where(m => m.NameEnglishSearch != null);
            }
        }

        private static IQueryable<StreetNameName> CompareByCompareType(IQueryable<StreetNameName> query, string searchValue, Language? language, bool isContainsFilter)
        {
            var containsValue = searchValue.SanitizeForBosaSearch();
            if (!language.HasValue)
            {
                return isContainsFilter
                    ? query.Where(i =>
                        i.NameDutchSearch.Contains(containsValue) ||
                        i.NameFrenchSearch.Contains(containsValue) ||
                        i.NameGermanSearch.Contains(containsValue) ||
                        i.NameEnglishSearch.Contains(containsValue))
                    : query.Where(i =>
                        i.NameDutch.Equals(searchValue) ||
                        i.NameFrench.Equals(searchValue) ||
                        i.NameGerman.Equals(searchValue) ||
                        i.NameEnglish.Equals(searchValue));
            }

            switch (language.Value)
            {
                default:
                case Language.Dutch:
                    return isContainsFilter
                        ? query.Where(i => i.NameDutchSearch.Contains(containsValue))
                        : query.Where(i => i.NameDutch.Equals(searchValue));

                case Language.French:
                    return isContainsFilter
                        ? query.Where(i => i.NameFrenchSearch.Contains(containsValue))
                        : query.Where(i => i.NameFrench.Equals(searchValue));

                case Language.German:
                    return isContainsFilter
                        ? query.Where(i => i.NameGermanSearch.Contains(containsValue))
                        : query.Where(i => i.NameGerman.Equals(searchValue));

                case Language.English:
                    return isContainsFilter
                        ? query.Where(i => i.NameEnglishSearch.Contains(containsValue))
                        : query.Where(i => i.NameEnglish.Equals(searchValue));
            }
        }
    }

    public sealed class StreetNameNameFilter
    {
        public string ObjectId { get; set; }
        public DateTimeOffset? StreetNameVersion { get; set; }
        public string StreetName { get; set; }
        public StreetNameStatus? Status { get; set; }
        public Language? Language { get; set; }
        public string MunicipalityObjectId { get; set; }
        public string MunicipalityVersion { get; set; }
        public bool IsContainsFilter { get; set; }

        public StreetNameNameFilter(BosaStreetNameRequest request)
        {
            ObjectId = request?.StraatnaamCode?.ObjectId;
            StreetNameVersion = request?.StraatnaamCode?.VersieId;
            StreetName = request?.Straatnaam?.Spelling;
            Language = request?.Straatnaam?.Taal?.ConvertToStreetNameLanguage();
            Status = request?.StraatnaamStatus?.ConvertToStreetNameStatus();
            MunicipalityObjectId = request?.GemeenteCode?.ObjectId;
            MunicipalityVersion = request?.GemeenteCode?.VersieId != null ? new Rfc3339SerializableDateTimeOffset(request.GemeenteCode.VersieId.Value).ToString() : string.Empty;
            IsContainsFilter = (request?.Straatnaam?.SearchType ?? BosaSearchType.Bevat) == BosaSearchType.Bevat;
        }
    }
}
