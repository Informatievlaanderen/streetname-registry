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
    using Consumer.Read.Postal;
    using Convertors;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.StreetNameList;
    using Projections.Syndication;

    public sealed class StreetNameListQuery : Query<StreetNameListItem, StreetNameFilter>
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly ConsumerPostalContext _postalContext;

        protected override ISorting Sorting => new StreetNameSorting();

        public StreetNameListQuery(LegacyContext legacyContext, SyndicationContext syndicationContext, ConsumerPostalContext postalContext)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
            _postalContext = postalContext;
        }

        protected override IQueryable<StreetNameListItem> Filter(FilteringHeader<StreetNameFilter> filtering)
        {
            var streetNames = _legacyContext
                .StreetNameList
                .AsNoTracking()
                .OrderBy(x => x.PersistentLocalId)
                .Where(s => !s.Removed && s.Complete && s.PersistentLocalId != null);

            if (!filtering.ShouldFilter)
            {
                return streetNames;
            }

            streetNames = ApplyNisCodeFilter(streetNames, filtering.Filter.NisCode);
            streetNames = ApplyNameDutchFilter(streetNames, filtering.Filter.NameDutch);
            streetNames = ApplyNameEnglishFilter(streetNames, filtering.Filter.NameEnglish);
            streetNames = ApplyNameFrenchFilter(streetNames, filtering.Filter.NameFrench);
            streetNames = ApplyNameGermanFilter(streetNames, filtering.Filter.NameGerman);

            var filterMunicipalityName = filtering.Filter.MunicipalityName.RemoveDiacritics();
            if (!string.IsNullOrEmpty(filtering.Filter.MunicipalityName))
            {
                var municipalityNisCodes = _syndicationContext
                    .MunicipalityLatestItems
                    .AsNoTracking()
                    .Where(x => x.NameDutchSearch == filterMunicipalityName
                        || x.NameFrenchSearch == filterMunicipalityName
                        || x.NameEnglishSearch == filterMunicipalityName
                        || x.NameGermanSearch == filterMunicipalityName)
                    .Select(x => x.NisCode)
                    .ToList();

                streetNames = streetNames.Where(m => municipalityNisCodes.Contains(m.NisCode));
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
                if (Enum.TryParse(typeof(StraatnaamStatus), filtering.Filter.Status, true, out var status))
                {
                    if (status is StraatnaamStatus straatnaamStatus)
                    {
                        var streetNameStatus = straatnaamStatus.ConvertToStreetNameStatus();
                        streetNames = streetNames.Where(m => m.Status.HasValue && m.Status.Value == streetNameStatus);
                    }
                }
                else
                    //have to filter on EF cannot return new List<>().AsQueryable() cause non-EF provider does not support .CountAsync()
                {
                    streetNames = streetNames.Where(m => m.Status.HasValue && (int) m.Status.Value == -1);
                }
            }

            if (!string.IsNullOrWhiteSpace(filtering.Filter.PostalCode))
            {
                var postalConsumerItem = _postalContext.PostalConsumerItems.Find(filtering.Filter.PostalCode);
                if (postalConsumerItem?.NisCode != null)
                {
                    streetNames = streetNames.Where(m => m.NisCode == postalConsumerItem.NisCode);
                }
            }

            return streetNames;
        }

        private IQueryable<StreetNameListItem> ApplyNisCodeFilter(IQueryable<StreetNameListItem> streetNames, string? filterNisCode) => !string.IsNullOrEmpty(filterNisCode)
            ? streetNames.Where(x => x.NisCode == filterNisCode)
            : streetNames;

        private IQueryable<StreetNameListItem> ApplyNameDutchFilter(IQueryable<StreetNameListItem> streetNames, string? filterName) => !string.IsNullOrEmpty(filterName)
            ? streetNames.Where(x => (x.NameDutch ?? "").Contains(filterName))
            : streetNames;

        private IQueryable<StreetNameListItem> ApplyNameEnglishFilter(IQueryable<StreetNameListItem> streetNames, string? filterName) => !string.IsNullOrEmpty(filterName)
            ? streetNames.Where(x => (x.NameEnglish ?? "").Contains(filterName))
            : streetNames;

        private IQueryable<StreetNameListItem> ApplyNameFrenchFilter(IQueryable<StreetNameListItem> streetNames, string? filterName) => !string.IsNullOrEmpty(filterName)
            ? streetNames.Where(x => (x.NameFrench ?? "").Contains(filterName))
            : streetNames;

        private IQueryable<StreetNameListItem> ApplyNameGermanFilter(IQueryable<StreetNameListItem> streetNames, string? filterName) => !string.IsNullOrEmpty(filterName)
            ? streetNames.Where(x => (x.NameEnglish ?? "").Contains(filterName))
            : streetNames;
    }

    public sealed class StreetNameSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(StreetNameListItem.NameDutch),
            nameof(StreetNameListItem.NameEnglish),
            nameof(StreetNameListItem.NameFrench),
            nameof(StreetNameListItem.NameGerman),
            nameof(StreetNameListItem.PersistentLocalId)
        };

        public SortingHeader DefaultSortingHeader { get; } =
            new SortingHeader(nameof(StreetNameListItem.PersistentLocalId), SortOrder.Ascending);
    }

    public sealed class StreetNameFilter
    {
        public string StreetNameName { get; set; } = string.Empty;
        public string MunicipalityName { get; set; } = string.Empty;
        public string NameDutch { get; set; } = string.Empty;
        public string NameFrench { get; set; } = string.Empty;
        public string NameGerman { get; set; } = string.Empty;
        public string NameEnglish { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? NisCode { get; set; } = string.Empty;
        public string? PostalCode { get; set; } = string.Empty;
    }
}
