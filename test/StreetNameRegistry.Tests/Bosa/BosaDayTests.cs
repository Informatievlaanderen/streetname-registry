namespace StreetNameRegistry.Tests.Bosa
{
    using System;
    using FluentAssertions;
    using NodaTime;
    using NodaTime.Text;
    using Projections.Bosa.Infrastructure;
    using Xunit;

    public class BosaDayTests
    {
        // The defaults: days start at 20:00 UTC.
        private readonly BosaDay _sut = new BosaDay(new BosaOptions { ChangesFrom = "2026-06-30T20:00:00Z" });

        [Theory]
        // summer time, the same moment as in winter time
        [InlineData("2026-07-01T19:59:59Z", "2026-07-01")]
        [InlineData("2026-07-01T20:00:00Z", "2026-07-02")]
        [InlineData("2026-07-01T23:30:00Z", "2026-07-02")]
        // winter time
        [InlineData("2026-12-01T19:59:59Z", "2026-12-01")]
        [InlineData("2026-12-01T20:00:00Z", "2026-12-02")]
        // DST changes
        [InlineData("2027-03-28T19:59:59Z", "2027-03-28")]
        [InlineData("2026-10-25T20:00:00Z", "2026-10-26")]
        // year boundary
        [InlineData("2026-12-31T20:00:00Z", "2027-01-01")]
        public void ChangeBelongsToDay(string instant, string expectedDay)
        {
            _sut.TryGetDay(InstantPattern.General.Parse(instant).Value, out var day).Should().BeTrue();
            day.Should().Be(DateOnly.Parse(expectedDay));
        }

        [Theory]
        // summer time, UTC+2
        [InlineData("2026-07-01T19:59:59Z", "2026-07-01")]
        [InlineData("2026-07-01T20:00:00Z", "2026-07-02")]
        // winter time, UTC+1
        [InlineData("2026-12-01T20:59:59Z", "2026-12-01")]
        [InlineData("2026-12-01T21:00:00Z", "2026-12-02")]
        public void WhenTimeZoneIsLocal_ThenDayFollowsLocalTime(string instant, string expectedDay)
        {
            var sut = new BosaDay(new BosaOptions { DayStartsAt = "22:00", TimeZone = "Europe/Brussels", ChangesFrom = "2026-06-30T22:00:00+02:00" });

            sut.TryGetDay(InstantPattern.General.Parse(instant).Value, out var day).Should().BeTrue();
            day.Should().Be(DateOnly.Parse(expectedDay));
        }

        [Fact]
        public void WhenChangeIsBeforeChangesFrom_ThenItBelongsToNoDay()
        {
            _sut.TryGetDay(Instant.FromUtc(2026, 6, 30, 19, 59, 59), out _).Should().BeFalse();
        }

        [Fact]
        public void WhenChangeIsAtChangesFrom_ThenItBelongsToTheFirstDay()
        {
            _sut.TryGetDay(Instant.FromUtc(2026, 6, 30, 20, 0), out var day).Should().BeTrue();
            day.Should().Be(new DateOnly(2026, 7, 1));
        }

        [Fact]
        public void WhenDayStartsAtMidnight_ThenCalendarDay()
        {
            var sut = new BosaDay(new BosaOptions { DayStartsAt = "00:00", TimeZone = "Europe/Brussels", ChangesFrom = "2026-01-01T00:00:00+01:00" });

            sut.TryGetDay(Instant.FromUtc(2026, 7, 1, 21, 59), out var before).Should().BeTrue();
            before.Should().Be(new DateOnly(2026, 7, 1));
            sut.TryGetDay(Instant.FromUtc(2026, 7, 1, 22, 0), out var after).Should().BeTrue();
            after.Should().Be(new DateOnly(2026, 7, 2));
        }

        [Fact]
        public void WhenChangesFromIsMissing_ThenThrows()
        {
            Action act = () => new BosaDay(new BosaOptions());

            act.Should().Throw<InvalidOperationException>();
        }
    }
}
