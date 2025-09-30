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
    using Converters;
    using List;
    using Microsoft.EntityFrameworkCore;
    using Municipality;
    using Projections.Legacy;

    public record StreetNameListViewQueryResponse(
        int StreetNamePersistentLocalId,
        Language? PrimaryLanguage,
        string? StreetNameDutch,
        string? StreetNameFrench,
        string? StreetNameEnglish,
        string? StreetNameGerman,
        string? HomonymAdditionDutch,
        string? HomonymAdditionFrench,
        string? HomonymAdditionEnglish,
        string? HomonymAdditionGerman,
        StreetNameStatus? Status,
        DateTimeOffset Version);

    public sealed class StreetNameListOsloQueryV2 : Be.Vlaanderen.Basisregisters.Api.Search.Query<StreetNameListView, StreetNameFilter, StreetNameListViewQueryResponse>
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

        protected override Expression<Func<StreetNameListView, StreetNameListViewQueryResponse>> Transformation =>
            streetName => new StreetNameListViewQueryResponse(
                streetName.StreetNamePersistentLocalId,
                streetName.PrimaryLanguage,
                streetName.StreetNameDutch,
                streetName.StreetNameFrench,
                streetName.StreetNameEnglish,
                streetName.StreetNameGerman,
                streetName.StreetNameHomonymAdditionDutch,
                streetName.StreetNameHomonymAdditionFrench,
                streetName.StreetNameHomonymAdditionEnglish,
                streetName.StreetNameHomonymAdditionGerman,
                streetName.StreetNameStatus,
                streetName.VersionTimestamp);
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
}
