namespace StreetNameRegistry.Api.Legacy.StreetName.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Convertors;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.StreetNameList;
    using Projections.Syndication;

    public class StreetNameListQuery : Query<StreetNameListItem, StreetNameFilter>
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;

        protected override ISorting Sorting => new StreetNameSorting();

        public StreetNameListQuery(LegacyContext legacyContext, SyndicationContext syndicationContext)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
        }

        protected override IQueryable<StreetNameListItem> Filter(FilteringHeader<StreetNameFilter> filtering)
        {
            var streetNames = _legacyContext
                .StreetNameList
                .AsNoTracking()
                .OrderBy(x => x.PersistentLocalId)
                .Where(s => !s.Removed && s.Complete && s.PersistentLocalId != null);

            if (!filtering.ShouldFilter)
                return streetNames;

            if (!string.IsNullOrEmpty(filtering.Filter.NisCode))
                streetNames = streetNames.Where(m => m.NisCode.Contains(filtering.Filter.NisCode));

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

                streetNames = streetNames.Where(x => municipalityNisCodes.Contains(x.NisCode));
            }

            if (!string.IsNullOrEmpty(filtering.Filter.Status))
            {
                if (Enum.TryParse(typeof(StraatnaamStatus), filtering.Filter.Status, true, out var status))
                {
                    var streetNameStatus = ((StraatnaamStatus)status).ConvertFromStraatnaamStatus();
                    streetNames = streetNames.Where(m => m.Status.HasValue && m.Status.Value == streetNameStatus);
                }
                else
                    //have to filter on EF cannot return new List<>().AsQueryable() cause non-EF provider does not support .CountAsync()
                    streetNames = streetNames.Where(m => m.Status.HasValue && (int)m.Status.Value == -1);
            }

            return streetNames;
        }
    }

    public class StreetNameSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(StreetNameListItem.NameDutch),
            nameof(StreetNameListItem.NameEnglish),
            nameof(StreetNameListItem.NameFrench),
            nameof(StreetNameListItem.NameGerman),
            nameof(StreetNameListItem.PersistentLocalId)
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(StreetNameListItem.PersistentLocalId), SortOrder.Ascending);
    }

    public class StreetNameFilter
    {
        public string MunicipalityName { get; set; }
        public string NameDutch { get; set; }
        public string NameFrench { get; set; }
        public string NameGerman { get; set; }
        public string NameEnglish { get; set; }
        public string Status { get; set; }
        public string? NisCode { get; set; }
    }
}
