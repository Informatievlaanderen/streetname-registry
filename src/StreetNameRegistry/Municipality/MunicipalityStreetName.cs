namespace StreetNameRegistry.Municipality
{
    using System;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using DataStructures;
    using Events;
    using Exceptions;

    public sealed partial class MunicipalityStreetName : Entity
    {
        public void Approve()
        {
            if (Status == StreetNameStatus.Current)
            {
                return;
            }

            GuardStreetNameStatus(StreetNameStatus.Proposed);

            Apply(new StreetNameWasApproved(_municipalityId, PersistentLocalId));
        }

        public void Reject()
        {
            if (Status == StreetNameStatus.Rejected)
            {
                return;
            }

            GuardStreetNameStatus(StreetNameStatus.Proposed);

            Apply(new StreetNameWasRejected(_municipalityId, PersistentLocalId));
        }

        public void Retire()
        {
            if (Status == StreetNameStatus.Retired)
            {
                return;
            }

            GuardStreetNameStatus(StreetNameStatus.Current);

            Apply(new StreetNameWasRetiredV2(_municipalityId, PersistentLocalId));
        }

        public void CorrectNames(Names names, Action<Names, HomonymAdditions, PersistentLocalId> guardStreetNameNames)
        {
            GuardStreetNameStatus(StreetNameStatus.Proposed, StreetNameStatus.Current);
            guardStreetNameNames(names, HomonymAdditions, PersistentLocalId);

            var correctedNames = new Names(names.Where(name => !Names.HasMatch(name.Language, name.Name)));
            if (!correctedNames.Any())
            {
                return;
            }

            Apply(new StreetNameNamesWereCorrected(_municipalityId, PersistentLocalId, correctedNames));
        }

        public void CorrectApproval()
        {
            if (Status == StreetNameStatus.Proposed)
            {
                return;
            }

            GuardStreetNameStatus(StreetNameStatus.Current);

            Apply(new StreetNameWasCorrectedFromApprovedToProposed(_municipalityId, PersistentLocalId));
        }

        public void CorrectRejection(Action guardUniqueActiveStreetNameNames)
        {
            if (Status == StreetNameStatus.Proposed)
            {
                return;
            }

            GuardStreetNameStatus(StreetNameStatus.Rejected);
            guardUniqueActiveStreetNameNames();

            Apply(new StreetNameWasCorrectedFromRejectedToProposed(_municipalityId, PersistentLocalId));
        }

        public void CorrectRetirement(Action guardUniqueActiveStreetNameNames)
        {
            if (Status == StreetNameStatus.Current)
            {
                return;
            }

            GuardStreetNameStatus(StreetNameStatus.Retired);
            guardUniqueActiveStreetNameNames();

            Apply(new StreetNameWasCorrectedFromRetiredToCurrent(_municipalityId, PersistentLocalId));
        }

        public void RestoreSnapshot(MunicipalityId municipalityId, StreetNameData streetNameData)
        {
            _municipalityId = municipalityId;

            PersistentLocalId = new PersistentLocalId(streetNameData.StreetNamePersistentLocalId);
            Status = streetNameData.Status;
            IsRemoved = streetNameData.IsRemoved;

            Names = new Names(streetNameData.Names);
            HomonymAdditions = new HomonymAdditions(streetNameData.HomonymAdditions);

            LegacyStreetNameId = streetNameData.LegacyStreetNameId is null
                ? null
                : new StreetNameId(streetNameData.LegacyStreetNameId.Value);

            _lastSnapshotEventHash = streetNameData.LastEventHash;
            _lastSnapshotProvenance = streetNameData.LastProvenanceData;
        }

        private void GuardStreetNameStatus(params StreetNameStatus[] validStatuses)
        {
            if (!validStatuses.Contains(Status))
            {
                throw new StreetNameHasInvalidStatusException(PersistentLocalId);
            }
        }
    }
}
