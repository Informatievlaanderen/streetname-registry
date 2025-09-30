namespace StreetNameRegistry.Projections.Elastic.StreetNameList
{
    using System;
    using System.Text.Json.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using NodaTime;
    using StreetNameRegistry.Municipality;

    public class StreetNameListPartialDocument
    {
        public DateTimeOffset VersionTimestamp { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public StreetNameStatus? Status { get; set; }

        public StreetNameListPartialDocument(Instant versionTimestamp)
            : this(versionTimestamp.ToBelgianDateTimeOffset())
        {
        }

        public StreetNameListPartialDocument(DateTimeOffset versionTimestamp)
        {
            VersionTimestamp = versionTimestamp;
        }
    }
}
