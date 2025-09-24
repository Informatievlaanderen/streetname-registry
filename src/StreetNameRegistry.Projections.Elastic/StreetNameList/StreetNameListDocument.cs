namespace StreetNameRegistry.Projections.Elastic.StreetNameList
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using NodaTime;
    using StreetNameRegistry.Infrastructure.Elastic;
    using StreetNameRegistry.Municipality;
    using Syndication.Municipality;
    using Language = StreetNameRegistry.Infrastructure.Elastic.Language;

    public sealed class StreetNameListDocument
    {
        public int StreetNamePersistentLocalId { get; set; }

        public Municipality Municipality { get; set; }

        public Name[] Names { get; set; }
        public Name[] HomonymAdditions { get; set; }
        public StreetNameStatus? Status { get; set; }

        public DateTimeOffset VersionTimestamp { get; set; }

        public StreetNameListDocument()
        { }

        public StreetNameListDocument(
            int streetNamePersistentLocalId,
            Municipality municipality,
            Name[] names,
            Name[] homonymAdditions,
            StreetNameStatus? status,
            Instant versionTimestamp)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            Municipality = municipality;
            Names = names;
            HomonymAdditions = homonymAdditions;
            Status = status;
            VersionTimestamp = versionTimestamp.ToBelgianDateTimeOffset();
        }
    }

    public sealed class Municipality
    {
        public string NisCode { get; set; }
        public Name[] Names { get; set; }
        public Language? PrimaryLanguage { get; set; }
        public bool IsInFlemishRegion { get; set; }

        public Municipality()
        {
        }

        public Municipality(string nisCode, IEnumerable<Name> names, Language? primaryLanguage, bool isInFlemishRegion)
        {
            NisCode = nisCode;
            Names = names.ToArray();
            PrimaryLanguage = primaryLanguage;
            IsInFlemishRegion = isInFlemishRegion;
        }

        public static Municipality FromMunicipalityLatestItem(MunicipalityLatestItem municipalityLatestItem)
        {
             return new Municipality(
                 municipalityLatestItem.NisCode,
                 new[]
                     {
                         new Name(municipalityLatestItem.NameDutch, StreetNameRegistry.Infrastructure.Elastic.Language.nl),
                         new Name(municipalityLatestItem.NameFrench, StreetNameRegistry.Infrastructure.Elastic.Language.fr),
                         new Name(municipalityLatestItem.NameGerman, StreetNameRegistry.Infrastructure.Elastic.Language.de),
                         new Name(municipalityLatestItem.NameEnglish, StreetNameRegistry.Infrastructure.Elastic.Language.en)
                     }
                     .Where(x => !string.IsNullOrEmpty(x.Spelling)),
                 ConvertToLanguage(municipalityLatestItem.PrimaryLanguage),
                 RegionFilter.IsFlemishRegion(municipalityLatestItem.NisCode));
        }

        private static Language? ConvertToLanguage(Taal? taal)
        {
            if (taal is null)
            {
                return null;
            }

            switch (taal.Value)
            {
                case Taal.NL:
                    return Language.nl;
                case Taal.FR:
                    return Language.fr;
                case Taal.DE:
                    return Language.de;
                case Taal.EN:
                    return Language.en;
            }

            throw new NotImplementedException($"Unknown language '{taal}'");
        }
    }
}
