namespace StreetNameRegistry.Projections.Elastic.StreetNameList
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Legacy.StreetNameListV2;
    using NodaTime;
    using StreetNameRegistry.Infrastructure.Elastic;
    using StreetNameRegistry.Municipality;
    using Language = StreetNameRegistry.Infrastructure.Elastic.Language;

    public sealed class StreetNameListDocument
    {
        public int StreetNamePersistentLocalId { get; set; }

        public Municipality Municipality { get; set; }

        public Name[] Names { get; set; }
        public Name[] SearchNames { get; set; }
        public Name[] HomonymAdditions { get; set; }
        public StreetNameStatus? Status { get; set; }
        public Language? PrimaryLanguage { get; set; }
        public bool IsInFlemishRegion { get; set; }

        public DateTimeOffset VersionTimestamp { get; set; }

        public StreetNameListDocument()
        { }

        public StreetNameListDocument(
            int streetNamePersistentLocalId,
            Municipality municipality,
            Name[] names,
            Name[] searchNames,
            Name[] homonymAdditions,
            StreetNameStatus? status,
            //Language? primaryLanguage,
            bool isInFlemishRegion,
            Instant versionTimestamp)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            Municipality = municipality;
            Names = names;
            SearchNames = searchNames;
            HomonymAdditions = homonymAdditions;
            Status = status;
            //PrimaryLanguage = primaryLanguage;
            IsInFlemishRegion = isInFlemishRegion;
            VersionTimestamp = versionTimestamp.ToBelgianDateTimeOffset();
        }
    }

    public sealed class Municipality
    {
        public string NisCode { get; set; }
        public Name[] Names { get; set; }

        public Municipality()
        { }

        public Municipality(string nisCode, IEnumerable<Name> names)
        {
            NisCode = nisCode;
            Names = names.ToArray();
        }

        public static Municipality FromMunicipalityLatestItem(StreetNameListMunicipality municipalityLatestItem)
        {
            throw new NotImplementedException();//TODO-pr talen ontbreken in legacy projection
            // return new Municipality(
            //     municipalityLatestItem.NisCode,
            //     new[]
            //         {
            //             new Name(municipalityLatestItem.NameDutch, StreetNameRegistry.Infrastructure.Elastic.Language.nl),
            //             new Name(municipalityLatestItem.NameFrench, StreetNameRegistry.Infrastructure.Elastic.Language.fr),
            //             new Name(municipalityLatestItem.NameGerman, StreetNameRegistry.Infrastructure.Elastic.Language.de),
            //             new Name(municipalityLatestItem.NameEnglish, StreetNameRegistry.Infrastructure.Elastic.Language.en)
            //         }
            //         .Where(x => !string.IsNullOrEmpty(x.Spelling)));
        }
    }

    // public sealed class StreetName
    // {
    //     public int StreetNamePersistentLocalId { get; set; }
    //     public Name[] Names { get; set; }
    //
    //     public Name[] HomonymAdditions { get; set; }
    //
    //     public StreetName()
    //     { }
    //
    //     public StreetName(int streetNamePersistentLocalId, IEnumerable<Name> names, IEnumerable<Name> homonymAdditions)
    //     {
    //         StreetNamePersistentLocalId = streetNamePersistentLocalId;
    //         Names = names.ToArray();
    //         HomonymAdditions = homonymAdditions.ToArray();
    //     }
    //
    //     public static StreetName FromStreetNameLatestItem(StreetNameLatestItem streetName)
    //     {
    //         return new StreetName(
    //             streetName.PersistentLocalId,
    //             new[]
    //                 {
    //                     new Name(streetName.NameDutch, Language.nl),
    //                     new Name(streetName.NameFrench, Language.fr),
    //                     new Name(streetName.NameGerman, Language.de),
    //                     new Name(streetName.NameEnglish, Language.en)
    //                 }
    //                 .Where(x => !string.IsNullOrEmpty(x.Spelling)),
    //             new[]
    //                 {
    //                     new Name(streetName.HomonymAdditionDutch, Language.nl),
    //                     new Name(streetName.HomonymAdditionFrench, Language.fr),
    //                     new Name(streetName.HomonymAdditionGerman, Language.de),
    //                     new Name(streetName.HomonymAdditionEnglish, Language.en)
    //                 }
    //                 .Where(x => !string.IsNullOrEmpty(x.Spelling))
    //         );
    //     }
    // }
}
