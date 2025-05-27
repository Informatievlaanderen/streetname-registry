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

    public sealed class StreetNameListOsloQueryV2 : Be.Vlaanderen.Basisregisters.Api.Search.Query<StreetNameListItemV2, StreetNameFilter>
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly ConsumerPostalContext _postalContext;

        protected override ISorting Sorting => new StreetNameSorting();

        public StreetNameListOsloQueryV2(
            LegacyContext legacyContext,
            SyndicationContext syndicationContext,
            ConsumerPostalContext postalContext)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
            _postalContext = postalContext;
        }

        protected override IQueryable<StreetNameListItemV2> Filter(FilteringHeader<StreetNameFilter> filtering)
        {
            IQueryable<StreetNameListItemV2>? streetNames = default;

            streetNames = _legacyContext
                .StreetNameListV2
                .AsNoTracking()
                .OrderBy(x => x.PersistentLocalId)
                .Where(s => !s.Removed);

            if (streetNames is null)
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
            {
                streetNames = streetNames.Where(s => s.NameDutch.Contains(filtering.Filter.NameDutch));
            }

            if (!string.IsNullOrEmpty(filtering.Filter.NameEnglish))
            {
                streetNames = streetNames.Where(s => s.NameEnglish.Contains(filtering.Filter.NameEnglish));
            }

            if (!string.IsNullOrEmpty(filtering.Filter.NameFrench))
            {
                streetNames = streetNames.Where(s => s.NameFrench.Contains(filtering.Filter.NameFrench));
            }

            if (!string.IsNullOrEmpty(filtering.Filter.NameGerman))
            {
                streetNames = streetNames.Where(s => s.NameGerman.Contains(filtering.Filter.NameGerman));
            }

            if (filtering.Filter.IsInFlemishRegion.HasValue)
            {
                streetNames = streetNames.Where(x => x.IsInFlemishRegion);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.MunicipalityName))
            {
                var filterMunicipalityName = filtering.Filter.MunicipalityName.RemoveDiacritics();
                var municipalityNisCodes = _syndicationContext
                    .MunicipalityLatestItems
                    .AsNoTracking()
                    .Where(x => x.NameDutchSearch == filterMunicipalityName ||
                                x.NameFrenchSearch == filterMunicipalityName ||
                                x.NameEnglishSearch == filterMunicipalityName ||
                                x.NameGermanSearch == filterMunicipalityName)
                    .Select(x => x.NisCode)
                    .ToList();

                if (streetNames is IQueryable<StreetNameListItemV2> streetNamesV2)
                {
                    if (municipalityNisCodes.Count <= 10)
                    {
                        var predicate = PredicateBuilder.False<StreetNameListItemV2>();
                        foreach (var nisCode in municipalityNisCodes)
                            predicate = predicate.Or(m => m.NisCode == nisCode);

                        streetNames = streetNames.Where(predicate);
                    }
                    else
                    {
                        streetNames = streetNames.Where(m => municipalityNisCodes.Contains(m.NisCode));
                    }
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

            if (!string.IsNullOrWhiteSpace(filtering.Filter.PostalCode))
            {
                var postalConsumerItem = _postalContext.PostalConsumerItems.Find(filtering.Filter.PostalCode);
                if (postalConsumerItem?.NisCode != null)
                {
                    streetNames = streetNames.Where(m => m.NisCode == postalConsumerItem.NisCode);
                }
                else
                {
                    streetNames = streetNames.Where(m => m.NisCode == "-1");
                }
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

        public SortingHeader DefaultSortingHeader { get; } =
            new SortingHeader(nameof(StreetNameListItem.PersistentLocalId), SortOrder.Ascending);
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
        public string? PostalCode { get; set; }
        public bool? IsInFlemishRegion { get; set; } = null;
    }

    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, bool>>
                (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}
