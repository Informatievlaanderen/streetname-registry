namespace StreetNameRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    [Obsolete("This is a legacy command and should not be used anymore.")]
    public sealed class ImportStreetNameFromCrab : IHasCrabProvenance
    {
        private static readonly Guid Namespace = new Guid("5905bc4c-d70d-4c43-8855-3aa9f962524b");

        public CrabStreetNameId StreetNameId { get; }
        public CrabMunicipalityId MunicipalityId { get; }
        public NisCode NisCode { get; }
        public CrabStreetName PrimaryStreetName { get; }
        public CrabStreetName SecondaryStreetName { get; }
        public CrabTransStreetName PrimaryTransStreetName { get; }
        public CrabTransStreetName SecondaryTransStreetName { get; }
        public CrabLanguage? PrimaryLanguage { get; }
        public CrabLanguage? SecondaryLanguage { get; }
        public CrabLifetime LifeTime { get; }
        public CrabTimestamp Timestamp { get; }
        public CrabOperator Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public ImportStreetNameFromCrab(
            CrabStreetNameId streetNameId,
            CrabMunicipalityId municipalityId,
            NisCode nisCode,
            CrabStreetName primaryStreetName,
            CrabStreetName secondaryStreetName,
            CrabTransStreetName primaryTransStreetName,
            CrabTransStreetName secondaryTransStreetName,
            CrabLanguage? primaryLanguage,
            CrabLanguage? secondaryLanguage,
            CrabLifetime lifeTime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            StreetNameId = streetNameId;
            MunicipalityId = municipalityId;
            NisCode = nisCode;
            PrimaryStreetName = primaryStreetName;
            SecondaryStreetName = secondaryStreetName;
            PrimaryTransStreetName = primaryTransStreetName;
            SecondaryTransStreetName = secondaryTransStreetName;
            PrimaryLanguage = primaryLanguage;
            SecondaryLanguage = secondaryLanguage;
            LifeTime = lifeTime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ImportStreetNameFromCrab-{ToString()}");

        public override string ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return StreetNameId;
            yield return MunicipalityId;
            yield return NisCode;
            yield return PrimaryStreetName;
            yield return SecondaryStreetName;
            yield return PrimaryTransStreetName;
            yield return SecondaryTransStreetName;
            yield return PrimaryLanguage;
            yield return SecondaryLanguage;
            yield return LifeTime.BeginDateTime.Print();
            yield return Timestamp;
            yield return Operator;
            yield return Modification;
            yield return Organisation;
        }
    }
}
