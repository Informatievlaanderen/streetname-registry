namespace StreetNameRegistry.Municipality
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using DataStructures;
    using Events;
    using Exceptions;

    public sealed partial class MunicipalityStreetName : Entity
    {
        public const int CorrectionChangeLimitPercentage = 30;

        public void Approve()
        {
            if (Status == StreetNameStatus.Current)
            {
                return;
            }

            GuardStreetNameStatus(StreetNameStatus.Proposed);

            Apply(new StreetNameWasApproved(_municipalityId, PersistentLocalId));
        }

        public void ApproveForMunicipalityMerger()
        {
            if (IsRemoved)
            {
                return;
            }

            if (DesiredStatusAfterMunicipalityMerger is not StreetNameStatus.Current)
            {
                return;
            }

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

        public void RejectForMunicipalityMerger(ICollection<PersistentLocalId> newPersistentLocalIds)
        {
            if (Status == StreetNameStatus.Rejected)
            {
                return;
            }

            GuardStreetNameStatus(StreetNameStatus.Proposed);

            Apply(new StreetNameWasRejectedBecauseOfMunicipalityMerger(_municipalityId, PersistentLocalId, newPersistentLocalIds));
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

        public void RetireForMunicipalityMerger(ICollection<PersistentLocalId> newPersistentLocalIds)
        {
            if (Status == StreetNameStatus.Retired)
            {
                return;
            }

            GuardStreetNameStatus(StreetNameStatus.Current);

            Apply(new StreetNameWasRetiredBecauseOfMunicipalityMerger(_municipalityId, PersistentLocalId, newPersistentLocalIds));
        }

        public void CorrectNames(Names namesToCorrect, Action<Names, HomonymAdditions, PersistentLocalId> guardStreetNameNames)
        {
            GuardStreetNameStatus(StreetNameStatus.Proposed, StreetNameStatus.Current);
            guardStreetNameNames(namesToCorrect, HomonymAdditions, PersistentLocalId);

            var correctedNames = new Names(namesToCorrect.Where(name => !Names.HasExactMatch(name.Language, name.Name)));
            if (!correctedNames.Any())
            {
                return;
            }

            foreach (var correctedName in namesToCorrect)
            {
                var originalName = Names.SingleOrDefault(x => x.Language == correctedName.Language);

                // This should never happen in the normal flow, but with CRAB migration there are streetnames without names in supported languages
                if (originalName is null)
                {
                    continue;
                }

                var changeDifference = LevenshteinDistanceCalculator.CalculatePercentage(correctedName.Name, originalName.Name);

                if (changeDifference > CorrectionChangeLimitPercentage)
                {
                    throw new StreetNameNameCorrectionExceededCharacterChangeLimitException(correctedName.Name);
                }
            }

            Apply(new StreetNameNamesWereCorrected(_municipalityId, PersistentLocalId, correctedNames));
        }

        public void ChangeNames(Names names, Action<Names, HomonymAdditions, PersistentLocalId> guardStreetNameNames)
        {
            GuardStreetNameStatus(StreetNameStatus.Proposed, StreetNameStatus.Current);
            guardStreetNameNames(names, HomonymAdditions, PersistentLocalId);

            var changedNames = new Names(names.Where(name => !Names.HasMatch(name.Language, name.Name)));
            if (!changedNames.Any())
            {
                return;
            }

            Apply(new StreetNameNamesWereChanged(_municipalityId, PersistentLocalId, changedNames));
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

        public void CorrectRejection(Action<Names, HomonymAdditions, PersistentLocalId> guardUniqueActiveStreetNameNames)
        {
            if (Status == StreetNameStatus.Proposed)
            {
                return;
            }

            GuardStreetNameStatus(StreetNameStatus.Rejected);
            guardUniqueActiveStreetNameNames(Names, HomonymAdditions, PersistentLocalId);

            Apply(new StreetNameWasCorrectedFromRejectedToProposed(_municipalityId, PersistentLocalId));
        }

        public void CorrectRetirement(Action<Names, HomonymAdditions, PersistentLocalId> guardUniqueActiveStreetNameNames)
        {
            if (Status == StreetNameStatus.Current)
            {
                return;
            }

            GuardStreetNameStatus(StreetNameStatus.Retired);

            //if (IsRenamed)
            //{
            //    throw new StreetNameIsRenamedException(PersistentLocalId);
            //}

            guardUniqueActiveStreetNameNames(Names, HomonymAdditions, PersistentLocalId);

            Apply(new StreetNameWasCorrectedFromRetiredToCurrent(_municipalityId, PersistentLocalId));
        }

        public void CorrectHomonymAdditions(
            HomonymAdditions homonymAdditions,
            Action<Names, HomonymAdditions, PersistentLocalId> guardUniqueActiveStreetNameNames)
        {
            GuardStreetNameStatus(StreetNameStatus.Proposed, StreetNameStatus.Current);

            foreach (var item in homonymAdditions)
            {
                if (!HomonymAdditions.HasLanguage(item.Language))
                {
                    throw new CannotAddHomonymAdditionException(item.Language);
                }
            }

            guardUniqueActiveStreetNameNames(Names, homonymAdditions, PersistentLocalId);

            var changedHomonymAdditions = homonymAdditions.Except(HomonymAdditions).ToList();

            if (changedHomonymAdditions.Any())
            {
                Apply(new StreetNameHomonymAdditionsWereCorrected(_municipalityId, PersistentLocalId, changedHomonymAdditions));
            }
        }

        public void RemoveHomonymAdditions(
            List<Language> languages,
            Action<Names, HomonymAdditions, PersistentLocalId> guardUniqueActiveStreetNameNames)
        {
            GuardStreetNameStatus(StreetNameStatus.Proposed, StreetNameStatus.Current);

            var names =  new Names(Names.Where(x => languages.Contains(x.Language)));
            guardUniqueActiveStreetNameNames(names, new HomonymAdditions(), PersistentLocalId);

            var homonymAdditionsToRemove = languages.Where(x => HomonymAdditions.HasLanguage(x)).ToList();

            if (homonymAdditionsToRemove.Any())
            {
                Apply(new StreetNameHomonymAdditionsWereRemoved(_municipalityId, PersistentLocalId, homonymAdditionsToRemove));
            }
        }

        public void RestoreSnapshot(MunicipalityId municipalityId, StreetNameData streetNameData)
        {
            _municipalityId = municipalityId;

            PersistentLocalId = new PersistentLocalId(streetNameData.StreetNamePersistentLocalId);
            Status = streetNameData.Status;
            IsRemoved = streetNameData.IsRemoved;
            IsRenamed = streetNameData.IsRenamed;

            Names = new Names(streetNameData.Names);
            HomonymAdditions = new HomonymAdditions(streetNameData.HomonymAdditions);

            LegacyStreetNameId = streetNameData.LegacyStreetNameId is null
                ? null
                : new StreetNameId(streetNameData.LegacyStreetNameId.Value);

            MergedStreetNamePersistentLocalIds = streetNameData.MergedStreetNamePersistentLocalIds.Select(x => new PersistentLocalId(x)).ToList();
            DesiredStatusAfterMunicipalityMerger = streetNameData.DesiredStatusAfterMunicipalityMerger;

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
