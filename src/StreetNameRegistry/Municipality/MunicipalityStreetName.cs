namespace StreetNameRegistry.Municipality
{
    using System;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
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

            if (Status != StreetNameStatus.Proposed)
            {
                throw new StreetNameHasInvalidStatusException(PersistentLocalId);
            }

            Apply(new StreetNameWasApproved(_municipalityId, PersistentLocalId));
        }

        public void Reject()
        {
            if (Status == StreetNameStatus.Rejected)
            {
                return;
            }

            if (Status != StreetNameStatus.Proposed)
            {
                throw new StreetNameHasInvalidStatusException(PersistentLocalId);
            }

            Apply(new StreetNameWasRejected(_municipalityId, PersistentLocalId));
        }

        public void Retire()
        {
            if (Status == StreetNameStatus.Retired)
            {
                return;
            }

            if (Status != StreetNameStatus.Current)
            {
                throw new StreetNameHasInvalidStatusException(PersistentLocalId);
            }

            Apply(new StreetNameWasRetiredV2(_municipalityId, PersistentLocalId));
        }

        public void CorrectNames(Names names, Action<Names, PersistentLocalId> guardStreetNameNames)
        {
            var validStatuses = new[] { StreetNameStatus.Proposed, StreetNameStatus.Current };

            if (!validStatuses.Contains(Status))
            {
                throw new StreetNameHasInvalidStatusException(PersistentLocalId);
            }

            guardStreetNameNames(names, PersistentLocalId);

            var correctedNames = new Names(
                names.Where(name => !Names.HasMatch(name.Language, name.Name)));

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

            if (Status != StreetNameStatus.Current)
            {
                throw new StreetNameHasInvalidStatusException(PersistentLocalId);
            }

            Apply(new StreetNameWasCorrectedFromApprovedToProposed(_municipalityId, PersistentLocalId));
        }

        public void CorrectRejection(Action guardUniqueActiveStreetNameNames)
        {
            if (Status == StreetNameStatus.Proposed)
            {
                return;
            }

            if (Status != StreetNameStatus.Rejected)
            {
                throw new StreetNameHasInvalidStatusException(PersistentLocalId);
            }

            guardUniqueActiveStreetNameNames();

            Apply(new StreetNameWasCorrectedFromRejectedToProposed(_municipalityId, PersistentLocalId));
        }

        public void CorrectRetirement(Action guardUniqueActiveStreetNameNames)
        {
            switch (Status)
            {
                case StreetNameStatus.Current:
                    return;
                case StreetNameStatus.Proposed or StreetNameStatus.Rejected:
                    throw new StreetNameHasInvalidStatusException(PersistentLocalId);
                case StreetNameStatus.Retired:
                    guardUniqueActiveStreetNameNames();
                    Apply(new StreetNameWasCorrectedFromRetiredToCurrent(_municipalityId, PersistentLocalId));
                    break;
            }
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
    }
}
