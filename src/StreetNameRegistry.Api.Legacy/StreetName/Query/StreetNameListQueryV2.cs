namespace StreetNameRegistry.Api.Legacy.StreetName.Query
{
    using System;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Convertors;
    using global::Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.StreetNameListV2;
    using Projections.Syndication;

    public sealed class StreetNameListQueryV2 : Query<StreetNameListItemV2, StreetNameFilter>
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;

        protected override ISorting Sorting => new StreetNameSorting();

        public StreetNameListQueryV2(LegacyContext legacyContext, SyndicationContext syndicationContext)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
        }

        protected override IQueryable<StreetNameListItemV2> Filter(FilteringHeader<StreetNameFilter> filtering)
        {
            IQueryable<StreetNameListItemV2>? streetNames = default;

            streetNames = _legacyContext
                .StreetNameListV2
                .AsNoTracking()
                .OrderBy(x => x.PersistentLocalId)
                .Where(s => !s.Removed);

            if (streetNames == null)
            {
                throw new NotImplementedException();
            }

            if (!filtering.ShouldFilter)
            {
                return streetNames;
            }

            if (!string.IsNullOrEmpty(filtering.Filter.NisCode))
            {
                streetNames = streetNames.Where(m => m.NisCode == filtering.Filter.NisCode);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.NameDutch))
                streetNames = streetNames.Where(s => s.NameDutch.Contains(filtering.Filter.NameDutch));

            if (!string.IsNullOrEmpty(filtering.Filter.NameEnglish))
                streetNames = streetNames.Where(s => s.NameEnglish.Contains(filtering.Filter.NameEnglish));

            if (!string.IsNullOrEmpty(filtering.Filter.NameFrench))
                streetNames = streetNames.Where(s => s.NameFrench.Contains(filtering.Filter.NameFrench));

            if (!string.IsNullOrEmpty(filtering.Filter.NameGerman))
                streetNames = streetNames.Where(s => s.NameGerman.Contains(filtering.Filter.NameGerman));

            var filterMunicipalityName = filtering.Filter.MunicipalityName.RemoveDiacritics();
            if (!string.IsNullOrEmpty(filtering.Filter.MunicipalityName))
            {
                var municipalityNisCodes = _syndicationContext
                    .MunicipalityLatestItems
                    .AsNoTracking()
                    .Where(x => x.NameDutchSearch == filterMunicipalityName ||
                                x.NameFrenchSearch == filterMunicipalityName ||
                                x.NameEnglishSearch == filterMunicipalityName ||
                                x.NameGermanSearch == filterMunicipalityName)
                    .Select(x => x.NisCode)
                    .ToList();


                if ((streetNames is IQueryable<StreetNameListItemV2> streetNamesV2))
                {
                    streetNames = streetNames.Where(m => municipalityNisCodes.Contains(m.NisCode));
                }
            }

            var filterStreetName = filtering.Filter.StreetNameName.RemoveDiacritics();
            if (!string.IsNullOrEmpty(filtering.Filter.StreetNameName))
            {
                streetNames = streetNames
                    .Where(x => x.NameDutchSearch == filterStreetName ||
                                x.NameFrenchSearch == filterStreetName ||
                                x.NameEnglishSearch == filterStreetName ||
                                x.NameGermanSearch == filterStreetName);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.Status))
            {
                if (Enum.TryParse(typeof(StraatnaamStatus), filtering.Filter.Status, true, out var status) && status != null)
                {
                    var streetNameStatus = ((StraatnaamStatus)status).ConvertToMunicipalityStreetNameStatus();
                    streetNames = streetNames.Where(m => m.Status.HasValue && m.Status.Value == streetNameStatus);
                }
                else
                {
                    //have to filter on EF cannot return new List<>().AsQueryable() cause non-EF provider does not support .CountAsync()
                    streetNames = streetNames.Where(m => m.Status.HasValue && (int) m.Status.Value == -1);
                }
            }

            return streetNames;
        }
    }
}
