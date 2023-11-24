namespace StreetNameRegistry.StreetName.Events.Crab
{
    using System;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;

    [Obsolete("CRAB events are obsolete.")]
    [EventName("StreetNameStatusWasImportedFromCrab")]
    [EventDescription("Legacy event om tblStraatnaamstatus en tblStraatnaamstatus_hist te importeren.")]
    public sealed class StreetNameStatusWasImportedFromCrab : ICrabEvent, IHasCrabKey<int>, IMessage
    {
        [EventPropertyDescription("CRAB-identificator van de straatnaamstatus.")]
        public int StreetNameStatusId { get; }

        [EventPropertyDescription("CRAB-identificator van de straatnaam.")]
        public int StreetNameId { get; }

        [EventPropertyDescription("CRAB-straatnaamstatus.")]
        public CrabStreetNameStatus StreetNameStatus { get; }

        [EventPropertyDescription("Datum waarop het object is ontstaan in werkelijkheid.")]
        public LocalDateTime? BeginDateTime { get; }

        [EventPropertyDescription("Datum waarop het object in werkelijkheid ophoudt te bestaan.")]
        public LocalDateTime? EndDateTime { get; }

        [EventPropertyDescription("Tijdstip waarop het object werd ingevoerd in de databank.")]
        public Instant Timestamp { get; }

        [EventPropertyDescription("Operator door wie het object werd ingevoerd in de databank.")]
        public string Operator { get; }

        [EventPropertyDescription("Bewerking waarmee het object werd ingevoerd in de databank.")]
        public CrabModification? Modification { get; }

        [EventPropertyDescription("Organisatie die het object heeft ingevoerd in de databank.")]
        public CrabOrganisation? Organisation { get; }

        [EventPropertyDescription("Unieke sleutel.")]
        public int Key => StreetNameStatusId;

        public StreetNameStatusWasImportedFromCrab(
            CrabStreetNameStatusId streetNameStatusId,
            CrabStreetNameId streetNameId,
            CrabStreetNameStatus streetNameStatus,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            StreetNameStatusId = streetNameStatusId;
            StreetNameId = streetNameId;
            StreetNameStatus = streetNameStatus;
            BeginDateTime = lifetime.BeginDateTime;
            EndDateTime = lifetime.EndDateTime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        [JsonConstructor]
        private StreetNameStatusWasImportedFromCrab(
            int streetNameStatusId,
            int streetNameId,
            CrabStreetNameStatus streetNameStatus,
            LocalDateTime? beginDateTime,
            LocalDateTime? endDateTime,
            Instant timestamp,
            string @operator,
            CrabModification? modification,
            CrabOrganisation? organisation) :
            this (
                  new CrabStreetNameStatusId(streetNameStatusId),
                  new CrabStreetNameId(streetNameId),
                  streetNameStatus,
                  new CrabLifetime(beginDateTime, endDateTime),
                  new CrabTimestamp(timestamp),
                  new CrabOperator(@operator),
                  modification,
                  organisation) { }
    }
}
