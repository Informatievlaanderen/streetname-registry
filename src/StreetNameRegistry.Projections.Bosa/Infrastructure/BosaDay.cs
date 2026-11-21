namespace StreetNameRegistry.Projections.Bosa.Infrastructure
{
    using System;
    using NodaTime;
    using NodaTime.Text;

    /// <summary>
    /// Determines to which (subjective) day a change belongs.
    /// A day does not start at midnight, but at <see cref="BosaOptions.DayStartsAt"/>:
    /// with a start of 20:00, day D runs from D-1 20:00 until D 20:00 (exclusive).
    /// Changes before <see cref="BosaOptions.ChangesFrom"/> belong to no day, they only build up state.
    /// </summary>
    public sealed class BosaDay
    {
        private readonly DateTimeZone _timeZone;
        private readonly LocalTime _dayStartsAt;

        public Instant ChangesFrom { get; }

        public BosaDay(BosaOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ChangesFrom))
                throw new InvalidOperationException($"Bosa:{nameof(BosaOptions.ChangesFrom)} is required, otherwise replaying would create a file for every day in history.");

            _timeZone = DateTimeZoneProviders.Tzdb[options.TimeZone];
            _dayStartsAt = LocalTimePattern.CreateWithInvariantCulture("HH:mm").Parse(options.DayStartsAt).GetValueOrThrow();
            ChangesFrom = OffsetDateTimePattern.ExtendedIso.Parse(options.ChangesFrom).GetValueOrThrow().ToInstant();
        }

        public bool TryGetDay(Instant instant, out DateOnly day)
        {
            day = default;
            if (instant < ChangesFrom)
                return false;

            var local = instant.InZone(_timeZone).LocalDateTime;

            day = (_dayStartsAt != LocalTime.Midnight && local.TimeOfDay >= _dayStartsAt
                ? local.Date.PlusDays(1)
                : local.Date).ToDateOnly();

            return true;
        }
    }
}
