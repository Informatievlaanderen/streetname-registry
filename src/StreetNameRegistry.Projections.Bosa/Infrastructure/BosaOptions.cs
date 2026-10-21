namespace StreetNameRegistry.Projections.Bosa.Infrastructure
{
    public class BosaOptions
    {
        public string Namespace { get; set; } = string.Empty;
        public bool Enabled { get; set; }

        /// <summary>
        /// Time (in <see cref="TimeZone"/>) at which a new day starts, e.g. "20:00".
        /// Changes at or after this time belong to the next day.
        /// </summary>
        public string DayStartsAt { get; set; } = "20:00";

        /// <summary>
        /// UTC, so every day lasts 24 hours and starts at the same moment in summer and winter time:
        /// the file has to be at Bosa at 23:00 Brussels time, 20:00 UTC leaves at least an hour.
        /// </summary>
        public string TimeZone { get; set; } = "UTC";

        /// <summary>
        /// The projections replay all events to build up state, but only changes from this moment on end up in a day file.
        /// ISO 8601 with offset, e.g. "2026-10-20T20:00:00Z".
        /// </summary>
        public string? ChangesFrom { get; set; }

        /// <summary>
        /// When empty, files are kept in memory (local development).
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// Format for the S3 key, {0} is the day as <see cref="System.DateOnly"/>, {1} is "add" or "update".
        /// </summary>
        public string FileNameFormat { get; set; } = "{0:yyyyMMdd}/streetname-{1}.xml";
    }
}
