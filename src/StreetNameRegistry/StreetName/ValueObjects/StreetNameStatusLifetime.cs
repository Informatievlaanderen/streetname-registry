namespace StreetNameRegistry.StreetName
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    internal class StreetNameStatusLifetime : ValueObject<StreetNameStatusLifetime>, IComparable<StreetNameStatusLifetime>
    {
        public int? StatusId { get; }
        public StreetNameStatus? Status { get; }
        public DateTimeOffset? BeginDateTime { get; }

        public StreetNameStatusLifetime(StreetNameStatus status, int? statusId, DateTimeOffset? beginDateTime)
        {
            Status = status;
            StatusId = statusId;
            BeginDateTime = beginDateTime;
        }

        public static bool operator <(StreetNameStatusLifetime? left, StreetNameStatusLifetime? right) => left?.CompareTo(right) < 0;

        public static bool operator <=(StreetNameStatusLifetime? left, StreetNameStatusLifetime? right) => left?.CompareTo(right) <= 0;

        public static bool operator >(StreetNameStatusLifetime? left, StreetNameStatusLifetime? right) => left?.CompareTo(right) > 0;

        public static bool operator >=(StreetNameStatusLifetime? left, StreetNameStatusLifetime? right) => left?.CompareTo(right) >= 0;

        protected override IEnumerable<object> Reflect()
        {
            if (StatusId != null)
            {
                yield return StatusId;
            }

            if (Status != null)
            {
                yield return Status;
            }

            if (BeginDateTime != null)
            {
                yield return BeginDateTime;
            }
        }

        public int CompareTo(StreetNameStatusLifetime? other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            return Nullable.Compare(BeginDateTime, other.BeginDateTime);
        }
    }
}
