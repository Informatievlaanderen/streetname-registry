namespace StreetNameRegistry.StreetName.Events.Crab
{
    using System;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;

    [Obsolete("CRAB events are obsolete.")]
    [EventName("StreetNameWasImportedFromCrab")]
    [EventDescription("Legacy event om tblStraatnaam en tblStraatnaam_hist te importeren.")]
    public sealed class StreetNameWasImportedFromCrab : IMessage
    {
        [EventPropertyDescription("CRAB-identificator van de straatnaam.")]
        public int StreetNameId { get; }

        [EventPropertyDescription("CRAB-identificator van de gemeente.")]
        public int MunicipalityId { get; }

        [EventPropertyDescription("Straatnaam in de primaire officiële taal.")]
        public string PrimaryStreetName { get; }

        [EventPropertyDescription("Straatnaam in de secundaire officiële taal.")]
        public string SecondaryStreetName { get; }

        [EventPropertyDescription("Tijdelijke opslag van een straatnaam in de primaire officiële taal.")]
        public string PrimaryTransStreetName { get; }

        [EventPropertyDescription("Tijdelijke opslag van een straatnaam in de secundaire officiële taal.")]
        public string SecondaryTransStreetName { get; }

        [EventPropertyDescription("Primaire officiële taal (voluit, EN) van de straatnaam.")]
        public CrabLanguage? PrimaryLanguage { get; }

        [EventPropertyDescription("Secundaire officiële taal (voluit, EN) van de straatnaam.")]
        public CrabLanguage? SecondaryLanguage { get; }

        [EventPropertyDescription("Tijdstip waarop het object werd ingevoerd in de databank.")]
        public Instant Timestamp { get; set; }

        [EventPropertyDescription("Datum waarop het object is ontstaan in werkelijkheid.")]
        public LocalDateTime? BeginDateTime { get; }

        [EventPropertyDescription("Datum waarop het object in werkelijkheid ophoudt te bestaan.")]
        public LocalDateTime? EndDateTime { get; }

        [EventPropertyDescription("Operator door wie het object werd ingevoerd in de databank.")]
        public string Operator { get; }

        [EventPropertyDescription("Bewerking waarmee het object werd ingevoerd in de databank.")]
        public CrabModification? Modification { get; }

        [EventPropertyDescription("Organisatie die het object heeft ingevoerd in de databank.")]
        public CrabOrganisation? Organisation { get; }

        public StreetNameWasImportedFromCrab(
            CrabStreetNameId streetNameId,
            CrabMunicipalityId municipalityId,
            CrabStreetName primaryStreetName,
            CrabStreetName secondaryStreetName,
            CrabTransStreetName primaryTransStreetName,
            CrabTransStreetName secondaryTransStreetName,
            CrabLanguage? primaryLanguage,
            CrabLanguage? secondaryLanguage,
            CrabTimestamp timestamp,
            CrabLifetime lifetime,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            StreetNameId = streetNameId;
            MunicipalityId = municipalityId;
            PrimaryStreetName = primaryStreetName?.Name;
            SecondaryStreetName = secondaryStreetName?.Name;
            PrimaryTransStreetName = primaryTransStreetName?.TransStreetName;
            SecondaryTransStreetName = secondaryTransStreetName?.TransStreetName;
            PrimaryLanguage = primaryLanguage;
            SecondaryLanguage = secondaryLanguage;
            Timestamp = timestamp;
            BeginDateTime = lifetime.BeginDateTime;
            EndDateTime = lifetime.EndDateTime;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        [JsonConstructor]
        private StreetNameWasImportedFromCrab(
            int streetNameId,
            int municipalityId,
            string primaryStreetName,
            string secondaryStreetName,
            string primaryTransStreetName,
            string secondaryTransStreetName,
            CrabLanguage? primaryLanguage,
            CrabLanguage? secondaryLanguage,
            Instant timestamp,
            LocalDateTime? beginDateTime,
            LocalDateTime? endDateTime,
            string @operator,
            CrabModification? modification,
            CrabOrganisation? organisation) :
            this(
                new CrabStreetNameId(streetNameId),
                new CrabMunicipalityId(municipalityId),
                string.IsNullOrEmpty(primaryStreetName) ? null : new CrabStreetName(primaryStreetName, primaryLanguage),
                string.IsNullOrEmpty(secondaryStreetName) ? null : new CrabStreetName(secondaryStreetName, secondaryLanguage),
                string.IsNullOrEmpty(primaryTransStreetName) ? null : new CrabTransStreetName(primaryTransStreetName, primaryLanguage),
                string.IsNullOrEmpty(secondaryTransStreetName) ? null : new CrabTransStreetName(secondaryTransStreetName, secondaryLanguage),
                primaryLanguage,
                secondaryLanguage,
                new CrabTimestamp(timestamp),
                new CrabLifetime(beginDateTime, endDateTime),
                new CrabOperator(@operator),
                modification,
                organisation) { }
    }
}
