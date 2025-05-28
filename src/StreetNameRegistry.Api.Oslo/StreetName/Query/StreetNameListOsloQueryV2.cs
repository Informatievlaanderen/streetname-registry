namespace StreetNameRegistry.Api.Oslo.StreetName.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Consumer.Read.Postal;
    using Converters;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.StreetNameList;
    using Projections.Legacy.StreetNameListV2;
    using Projections.Syndication;

    public sealed class StreetNameListOsloQueryV2 : Be.Vlaanderen.Basisregisters.Api.Search.Query<StreetNameListView, StreetNameFilter>
    {
        private readonly LegacyContext _legacyContext;

        protected override ISorting Sorting => new StreetNameSorting();

        public StreetNameListOsloQueryV2(LegacyContext legacyContext)
        {
            _legacyContext = legacyContext;
        }

        protected override IQueryable<StreetNameListView> Filter(FilteringHeader<StreetNameFilter> filtering)
        {
            var streetNames = _legacyContext.StreetNameListView
                .AsNoTracking()
                .AsQueryable();

            if (!filtering.ShouldFilter)
            {
                return streetNames;
            }

            if (filtering.Filter.IsInFlemishRegion.HasValue)
            {
                streetNames = streetNames.Where(x => x.IsInFlemishRegion);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.Status))
            {
                if (Enum.TryParse<StraatnaamStatus>(filtering.Filter.Status, true, out var status))
                {
                    var streetNameStatus = status.ConvertToMunicipalityStreetNameStatus();
                    streetNames = streetNames.Where(m => m.StreetNameStatus.HasValue && m.StreetNameStatus.Value == streetNameStatus);
                }
                else
                {
                    //have to filter on EF cannot return new List<>().AsQueryable() cause non-EF provider does not support .CountAsync()
                    streetNames = streetNames.Where(m => m.StreetNameStatus.HasValue && (int)m.StreetNameStatus.Value == -1);
                }
            }

            if (!string.IsNullOrEmpty(filtering.Filter.NisCode))
            {
                streetNames = streetNames.Where(m => m.NisCode == filtering.Filter.NisCode);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.MunicipalityName))
            {
                var filterMunicipalityName = filtering.Filter.MunicipalityName.RemoveDiacritics();
                streetNames = streetNames
                    .Where(x =>
                        x.MunicipalityNameDutchSearch == filterMunicipalityName ||
                        x.MunicipalityNameFrenchSearch == filterMunicipalityName ||
                        x.MunicipalityNameEnglishSearch == filterMunicipalityName ||
                        x.MunicipalityNameGermanSearch == filterMunicipalityName);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.NameDutch))
            {
                streetNames = streetNames.Where(s => s.StreetNameDutch.Contains(filtering.Filter.NameDutch));
            }

            if (!string.IsNullOrEmpty(filtering.Filter.NameEnglish))
            {
                streetNames = streetNames.Where(s => s.StreetNameEnglish.Contains(filtering.Filter.NameEnglish));
            }

            if (!string.IsNullOrEmpty(filtering.Filter.NameFrench))
            {
                streetNames = streetNames.Where(s => s.StreetNameFrench.Contains(filtering.Filter.NameFrench));
            }

            if (!string.IsNullOrEmpty(filtering.Filter.NameGerman))
            {
                streetNames = streetNames.Where(s => s.StreetNameGerman.Contains(filtering.Filter.NameGerman));
            }

            var filterStreetName = filtering.Filter.StreetNameName.RemoveDiacritics();
            if (!string.IsNullOrEmpty(filtering.Filter.StreetNameName))
            {
                streetNames = streetNames
                    .Where(x =>
                        x.StreetNameDutchSearch == filterStreetName ||
                        x.StreetNameFrenchSearch == filterStreetName ||
                        x.StreetNameEnglishSearch == filterStreetName ||
                        x.StreetNameGermanSearch == filterStreetName);
            }

            return streetNames;
        }
    }

    public class StreetNameSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } =
        [
            nameof(StreetNameListView.StreetNameDutch),
            nameof(StreetNameListView.StreetNameEnglish),
            nameof(StreetNameListView.StreetNameFrench),
            nameof(StreetNameListView.StreetNameGerman),
            nameof(StreetNameListView.StreetNamePersistentLocalId)
        ];

        public SortingHeader DefaultSortingHeader { get; } =
            new SortingHeader(nameof(StreetNameListView.StreetNamePersistentLocalId), SortOrder.Ascending);
    }

    public class StreetNameFilter
    {
        public string StreetNameName { get; set; }
        public string MunicipalityName { get; set; }
        public string NameDutch { get; set; }
        public string NameFrench { get; set; }
        public string NameGerman { get; set; }
        public string NameEnglish { get; set; }
        public string Status { get; set; }
        public string? NisCode { get; set; }
        public bool? IsInFlemishRegion { get; set; } = null;
    }
}
