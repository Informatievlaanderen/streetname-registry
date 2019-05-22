namespace StreetNameRegistry.Api.Legacy.StreetName.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;
    using Projections.Legacy;
    using Projections.Legacy.StreetNameSyndication;

    public class StreetNameSyndicationQueryResult
    {
        public bool ContainsEvent { get; }
        public bool ContainsObject { get; }

        public Guid? StreetNameId { get; }
        public long Position { get; }
        public string ChangeType { get; }
        public int? OsloId { get; }
        public string NisCode { get; }
        public Instant RecordCreatedAt { get; }
        public Instant LastChangedOn { get; }
        public StreetNameStatus? Status { get; }
        public string NameDutch { get; }
        public string NameFrench { get; }
        public string NameGerman { get; }
        public string NameEnglish { get; }
        public string HomonymAdditionDutch { get; }
        public string HomonymAdditionFrench { get; }
        public string HomonymAdditionGerman { get; }
        public string HomonymAdditionEnglish { get; }
        public bool IsComplete { get; }
        public Organisation? Organisation { get; }
        public Plan? Plan { get; }
        public string EventDataAsXml { get; }

        public StreetNameSyndicationQueryResult(
            Guid? streetNameId,
            long position,
            int? osloId,
            string nisCode,
            string changeType,
            Instant recordCreatedAt,
            Instant lastChangedOn,
            bool isComplete,
            Organisation? organisation,
            Plan? plan)
        {
            ContainsEvent = false;
            ContainsObject = false;

            StreetNameId = streetNameId;
            Position = position;
            OsloId = osloId;
            NisCode = nisCode;
            ChangeType = changeType;
            RecordCreatedAt = recordCreatedAt;
            LastChangedOn = lastChangedOn;
            IsComplete = isComplete;
            Organisation = organisation;
            Plan = plan;
        }

        public StreetNameSyndicationQueryResult(
            Guid? streetNameId,
            long position,
            int? osloId,
            string nisCode,
            string changeType,
            Instant recordCreatedAt,
            Instant lastChangedOn,
            bool isComplete,
            Organisation? organisation,
            Plan? plan,
            string eventDataAsXml)
            : this(streetNameId,
                position,
                osloId,
                nisCode,
                changeType,
                recordCreatedAt,
                lastChangedOn,
                isComplete,
                organisation,
                plan)
        {
            ContainsEvent = true;
            EventDataAsXml = eventDataAsXml;
        }

        public StreetNameSyndicationQueryResult(
            Guid? streetNameId,
            long position,
            int? osloId,
            string nisCode,
            string changeType,
            Instant recordCreatedAt,
            Instant lastChangedOn,
            StreetNameStatus? status,
            string nameDutch,
            string nameFrench,
            string nameGerman,
            string nameEnglish,
            string homonymAdditionDutch,
            string homonymAdditionFrench,
            string homonymAdditionGerman,
            string homonymAdditionEnglish,
            bool isComplete,
            Organisation? organisation,
            Plan? plan)
            : this(
                streetNameId,
                position,
                osloId,
                nisCode,
                changeType,
                recordCreatedAt,
                lastChangedOn,
                isComplete,
                organisation,
                plan)
        {
            ContainsObject = true;

            Status = status;
            NameDutch = nameDutch;
            NameFrench = nameFrench;
            NameGerman = nameGerman;
            NameEnglish = nameEnglish;
            HomonymAdditionDutch = homonymAdditionDutch;
            HomonymAdditionFrench = homonymAdditionFrench;
            HomonymAdditionGerman = homonymAdditionGerman;
            HomonymAdditionEnglish = homonymAdditionEnglish;
        }

        public StreetNameSyndicationQueryResult(
            Guid? streetNameId,
            long position,
            int? osloId,
            string nisCode,
            string changeType,
            Instant recordCreatedAt,
            Instant lastChangedOn,
            StreetNameStatus? status,
            string nameDutch,
            string nameFrench,
            string nameGerman,
            string nameEnglish,
            string homonymAdditionDutch,
            string homonymAdditionFrench,
            string homonymAdditionGerman,
            string homonymAdditionEnglish,
            bool isComplete,
            Organisation? organisation,
            Plan? plan,
            string eventDataAsXml)
            : this(
                streetNameId,
                position,
                osloId,
                nisCode,
                changeType,
                recordCreatedAt,
                lastChangedOn,
                status,
                nameDutch,
                nameFrench,
                nameGerman,
                nameEnglish,
                homonymAdditionDutch,
                homonymAdditionFrench,
                homonymAdditionGerman,
                homonymAdditionEnglish,
                isComplete,
                organisation,
                plan)
        {
            ContainsObject = true;
            ContainsEvent = true;

            EventDataAsXml = eventDataAsXml;
        }
    }

    public class StreetNameSyndicationQuery : Query<StreetNameSyndicationItem, StreetNameSyndicationFilter,
        StreetNameSyndicationQueryResult>
    {
        private readonly LegacyContext _context;
        private readonly bool _embedEvent;
        private readonly bool _embedObject;

        public StreetNameSyndicationQuery(
            LegacyContext context,
            bool embedEvent,
            bool embedObject)
        {
            _context = context;
            _embedEvent = embedEvent;
            _embedObject = embedObject;
        }

        protected override ISorting Sorting => new StreetNameSyndicationSorting();

        protected override Expression<Func<StreetNameSyndicationItem, StreetNameSyndicationQueryResult>> Transformation
        {
            get
            {
                if (_embedEvent && _embedObject)
                    return x => new StreetNameSyndicationQueryResult(
                        x.StreetNameId,
                        x.Position,
                        x.OsloId,
                        x.NisCode,
                        x.ChangeType,
                        x.RecordCreatedAt,
                        x.LastChangedOn,
                        x.Status,
                        x.NameDutch,
                        x.NameFrench,
                        x.NameGerman,
                        x.NameEnglish,
                        x.HomonymAdditionDutch,
                        x.HomonymAdditionFrench,
                        x.HomonymAdditionGerman,
                        x.HomonymAdditionEnglish,
                        x.IsComplete,
                        x.Organisation,
                        x.Plan,
                        x.EventDataAsXml);

                if (_embedEvent)
                    return x => new StreetNameSyndicationQueryResult(
                        x.StreetNameId,
                        x.Position,
                        x.OsloId,
                        x.NisCode,
                        x.ChangeType,
                        x.RecordCreatedAt,
                        x.LastChangedOn,
                        x.IsComplete,
                        x.Organisation,
                        x.Plan,
                        x.EventDataAsXml);

                if (_embedObject)
                    return x => new StreetNameSyndicationQueryResult(
                        x.StreetNameId,
                        x.Position,
                        x.OsloId,
                        x.NisCode,
                        x.ChangeType,
                        x.RecordCreatedAt,
                        x.LastChangedOn,
                        x.Status,
                        x.NameDutch,
                        x.NameFrench,
                        x.NameGerman,
                        x.NameEnglish,
                        x.HomonymAdditionDutch,
                        x.HomonymAdditionFrench,
                        x.HomonymAdditionGerman,
                        x.HomonymAdditionEnglish,
                        x.IsComplete,
                        x.Organisation,
                        x.Plan);

                return x => new StreetNameSyndicationQueryResult(
                    x.StreetNameId,
                    x.Position,
                    x.OsloId,
                    x.NisCode,
                    x.ChangeType,
                    x.RecordCreatedAt,
                    x.LastChangedOn,
                    x.IsComplete,
                    x.Organisation,
                    x.Plan);
            }
        }

        protected override IQueryable<StreetNameSyndicationItem> Filter(FilteringHeader<StreetNameSyndicationFilter> filtering)
        {
            var streetNames = _context
                .StreetNameSyndication
                .AsNoTracking();

            if (!filtering.ShouldFilter)
                return streetNames;

            if (filtering.Filter.Position.HasValue)
                streetNames = streetNames.Where(m => m.Position >= filtering.Filter.Position);

            return streetNames;
        }
    }

    internal class StreetNameSyndicationSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(StreetNameSyndicationItem.Position)
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(StreetNameSyndicationItem.Position), SortOrder.Ascending);
    }

    public class StreetNameSyndicationFilter
    {
        public long? Position { get; set; }
        public string Embed { get; set; }

        public bool ContainsEvent =>
            Embed.Contains("event", StringComparison.OrdinalIgnoreCase);

        public bool ContainsObject =>
            Embed.Contains("object", StringComparison.OrdinalIgnoreCase);
    }
}
