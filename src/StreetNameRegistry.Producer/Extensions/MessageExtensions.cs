namespace StreetNameRegistry.Producer.Extensions
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Municipality.Events;
    using Contracts = Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
    using ContractsCommon = Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using Domain = StreetName.Events;

    public static class MessageExtensions
    {
        private static ContractsCommon.Provenance ToContract(this ProvenanceData provenance) => new ContractsCommon.Provenance(
            provenance.Timestamp.ToString(),
            provenance.Application.ToString(),
            provenance.Modification.ToString(),
            provenance.Organisation.ToString(),
            provenance.Reason);

        public static Contracts.StreetNameBecameComplete ToContract(this Domain.StreetNameBecameComplete message) =>
            new Contracts.StreetNameBecameComplete(message.StreetNameId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.StreetNameBecameCurrent ToContract(this Domain.StreetNameBecameCurrent message) =>
            new Contracts.StreetNameBecameCurrent(message.StreetNameId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.StreetNameBecameIncomplete ToContract(this Domain.StreetNameBecameIncomplete message) =>
            new Contracts.StreetNameBecameIncomplete(message.StreetNameId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.StreetNameHomonymAdditionWasCleared ToContract(this Domain.StreetNameHomonymAdditionWasCleared message) =>
            new Contracts.StreetNameHomonymAdditionWasCleared(message.StreetNameId.ToString("D"), message.Language.ToString(), message.Provenance.ToContract());

        public static Contracts.StreetNameHomonymAdditionWasCorrected ToContract(this Domain.StreetNameHomonymAdditionWasCorrected message) =>
            new Contracts.StreetNameHomonymAdditionWasCorrected(message.StreetNameId.ToString("D"), message.HomonymAddition, message.Language.ToString(), message.Provenance.ToContract());

        public static Contracts.StreetNameHomonymAdditionWasCorrectedToCleared ToContract(this Domain.StreetNameHomonymAdditionWasCorrectedToCleared message) =>
            new Contracts.StreetNameHomonymAdditionWasCorrectedToCleared(message.StreetNameId.ToString("D"), message.Language.ToString(), message.Provenance.ToContract());

        public static Contracts.StreetNameHomonymAdditionWasDefined ToContract(this Domain.StreetNameHomonymAdditionWasDefined message) =>
            new Contracts.StreetNameHomonymAdditionWasDefined(message.StreetNameId.ToString("D"), message.HomonymAddition, message.Language.ToString(), message.Provenance.ToContract());

        public static Contracts.StreetNameNameWasCleared ToContract(this Domain.StreetNameNameWasCleared message) =>
            new Contracts.StreetNameNameWasCleared(message.StreetNameId.ToString("D"), message.Language.ToString(), message.Provenance.ToContract());

        public static Contracts.StreetNameNameWasCorrected ToContract(this Domain.StreetNameNameWasCorrected message) =>
            new Contracts.StreetNameNameWasCorrected(message.StreetNameId.ToString("D"), message.Name, message.Language.ToString(), message.Provenance.ToContract());

        public static Contracts.StreetNameNameWasCorrectedToCleared ToContract(this Domain.StreetNameNameWasCorrectedToCleared message) =>
            new Contracts.StreetNameNameWasCorrectedToCleared(message.StreetNameId.ToString("D"), message.Language.ToString(), message.Provenance.ToContract());

        public static Contracts.StreetNamePersistentLocalIdWasAssigned ToContract(this Domain.StreetNamePersistentLocalIdWasAssigned message) =>
            new Contracts.StreetNamePersistentLocalIdWasAssigned(message.StreetNameId.ToString("D"), message.PersistentLocalId,message.AssignmentDate.ToString(), message.Provenance.ToContract());

        public static Contracts.StreetNamePrimaryLanguageWasCleared ToContract(this Domain.StreetNamePrimaryLanguageWasCleared message) =>
            new Contracts.StreetNamePrimaryLanguageWasCleared(message.StreetNameId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.StreetNamePrimaryLanguageWasCorrected ToContract(this Domain.StreetNamePrimaryLanguageWasCorrected message) =>
            new Contracts.StreetNamePrimaryLanguageWasCorrected(message.StreetNameId.ToString("D"), message.PrimaryLanguage.ToString(), message.Provenance.ToContract());

        public static Contracts.StreetNamePrimaryLanguageWasCorrectedToCleared ToContract(this Domain.StreetNamePrimaryLanguageWasCorrectedToCleared message) =>
            new Contracts.StreetNamePrimaryLanguageWasCorrectedToCleared(message.StreetNameId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.StreetNamePrimaryLanguageWasDefined ToContract(this Domain.StreetNamePrimaryLanguageWasDefined message) =>
            new Contracts.StreetNamePrimaryLanguageWasDefined(message.StreetNameId.ToString("D"), message.PrimaryLanguage.ToString(), message.Provenance.ToContract());

        public static Contracts.StreetNameSecondaryLanguageWasCleared ToContract(this Domain.StreetNameSecondaryLanguageWasCleared message) =>
            new Contracts.StreetNameSecondaryLanguageWasCleared(message.StreetNameId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.StreetNameSecondaryLanguageWasCorrected ToContract(this Domain.StreetNameSecondaryLanguageWasCorrected message) =>
            new Contracts.StreetNameSecondaryLanguageWasCorrected(message.StreetNameId.ToString("D"), message.SecondaryLanguage.ToString(), message.Provenance.ToContract());

        public static Contracts.StreetNameSecondaryLanguageWasCorrectedToCleared ToContract(this Domain.StreetNameSecondaryLanguageWasCorrectedToCleared message) =>
            new Contracts.StreetNameSecondaryLanguageWasCorrectedToCleared(message.StreetNameId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.StreetNameSecondaryLanguageWasDefined ToContract(this Domain.StreetNameSecondaryLanguageWasDefined message) =>
            new Contracts.StreetNameSecondaryLanguageWasDefined(message.StreetNameId.ToString("D"), message.SecondaryLanguage.ToString(), message.Provenance.ToContract());

        public static Contracts.StreetNameStatusWasCorrectedToRemoved ToContract(this Domain.StreetNameStatusWasCorrectedToRemoved message) =>
            new Contracts.StreetNameStatusWasCorrectedToRemoved(message.StreetNameId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.StreetNameStatusWasRemoved ToContract(this Domain.StreetNameStatusWasRemoved message) =>
            new Contracts.StreetNameStatusWasRemoved(message.StreetNameId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.StreetNameWasCorrectedToCurrent ToContract(this Domain.StreetNameWasCorrectedToCurrent message) =>
            new Contracts.StreetNameWasCorrectedToCurrent(message.StreetNameId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.StreetNameWasCorrectedToProposed ToContract(this Domain.StreetNameWasCorrectedToProposed message) =>
            new Contracts.StreetNameWasCorrectedToProposed(message.StreetNameId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.StreetNameWasCorrectedToRetired ToContract(this Domain.StreetNameWasCorrectedToRetired message) =>
            new Contracts.StreetNameWasCorrectedToRetired(message.StreetNameId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.StreetNameWasMigrated ToContract(this Domain.StreetNameWasMigrated message) =>
            new Contracts.StreetNameWasMigrated(message.StreetNameId.ToString("D"), message.MunicipalityId.ToString("D"), message.PersistentLocalId, message.Provenance.ToContract());

        public static Contracts.StreetNameWasNamed ToContract(this Domain.StreetNameWasNamed message) =>
            new Contracts.StreetNameWasNamed(message.StreetNameId.ToString("D"), message.Name, message.Language.ToString(), message.Provenance.ToContract());

        public static Contracts.StreetNameWasProposed ToContract(this Domain.StreetNameWasProposed message) =>
            new Contracts.StreetNameWasProposed(message.StreetNameId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.StreetNameWasRegistered ToContract(this Domain.StreetNameWasRegistered message) =>
            new Contracts.StreetNameWasRegistered(message.StreetNameId.ToString("D"), message.MunicipalityId.ToString(), message.NisCode, message.Provenance.ToContract());

        public static Contracts.StreetNameWasRemoved ToContract(this Domain.StreetNameWasRemoved message) =>
            new Contracts.StreetNameWasRemoved(message.StreetNameId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.StreetNameWasRetired ToContract(this Domain.StreetNameWasRetired message) =>
            new Contracts.StreetNameWasRetired(message.StreetNameId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.StreetNameWasProposedV2 ToContract(this StreetNameWasProposedV2 message) =>
            new Contracts.StreetNameWasProposedV2(
                message.MunicipalityId.ToString("D"),
                message.NisCode,
                message.StreetNameNames.ToDictionary(x => x.Language.ToString(), x => x.Name),
                message.PersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.StreetNameWasMigratedToMunicipality ToContract(this StreetNameWasMigratedToMunicipality message) =>
            new Contracts.StreetNameWasMigratedToMunicipality(
                message.MunicipalityId.ToString("D"),
                message.NisCode,
                message.StreetNameId.ToString("D"),
                message.PersistentLocalId,
                message.Status.ToString(),
                message.PrimaryLanguage.ToString(),
                message.SecondaryLanguage.ToString(),
                message.Names.ToDictionary(x => x.Key.ToString(), x => x.Value),
                message.HomonymAdditions.ToDictionary(x => x.Key.ToString(), x => x.Value),
                message.IsCompleted,
                message.IsRemoved,
                message.Provenance.ToContract());

        public static Contracts.StreetNameWasApproved ToContract(this StreetNameWasApproved message) =>
            new Contracts.StreetNameWasApproved(message.MunicipalityId.ToString("D"), message.PersistentLocalId, message.Provenance.ToContract());

        public static Contracts.StreetNameWasCorrectedFromApprovedToProposed ToContract(this StreetNameWasCorrectedFromApprovedToProposed message) =>
            new Contracts.StreetNameWasCorrectedFromApprovedToProposed(message.MunicipalityId.ToString("D"), message.PersistentLocalId, message.Provenance.ToContract());

        public static Contracts.StreetNameWasRejected ToContract(this StreetNameWasRejected message) =>
            new Contracts.StreetNameWasRejected(message.MunicipalityId.ToString("D"), message.PersistentLocalId, message.Provenance.ToContract());

        public static Contracts.StreetNameWasCorrectedFromRejectedToProposed ToContract(this StreetNameWasCorrectedFromRejectedToProposed message) =>
            new Contracts.StreetNameWasCorrectedFromRejectedToProposed(message.MunicipalityId.ToString("D"), message.PersistentLocalId, message.Provenance.ToContract());

        public static Contracts.StreetNameWasRetiredV2 ToContract(this StreetNameWasRetiredV2 message) =>
            new Contracts.StreetNameWasRetiredV2(message.MunicipalityId.ToString("D"), message.PersistentLocalId, message.Provenance.ToContract());

        public static Contracts.StreetNameWasCorrectedFromRetiredToCurrent ToContract(this StreetNameWasCorrectedFromRetiredToCurrent message) =>
            new Contracts.StreetNameWasCorrectedFromRetiredToCurrent(message.MunicipalityId.ToString("D"), message.PersistentLocalId, message.Provenance.ToContract());

        public static Contracts.StreetNameNamesWereCorrected ToContract(this StreetNameNamesWereCorrected message) =>
            new Contracts.StreetNameNamesWereCorrected(
                message.MunicipalityId.ToString("D"),
                message.PersistentLocalId,
                message.StreetNameNames.ToDictionary(x => x.Language.ToString(), x => x.Name),
                message.Provenance.ToContract());
    }
}
