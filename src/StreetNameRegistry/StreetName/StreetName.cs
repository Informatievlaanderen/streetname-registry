namespace StreetNameRegistry.StreetName
{
    using System;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Events;
    using Events.Crab;

    public partial class StreetName : AggregateRootEntity
    {
        public static readonly Func<StreetName> Factory = () => new StreetName();

        public bool IsRetired => _status == StreetNameStatus.Retired;

        public static StreetName Register(StreetNameId streetNameId, MunicipalityId municipalityId, NisCode nisCode)
        {
            var streetName = Factory();
            streetName.ApplyChange(new StreetNameWasRegistered(streetNameId, municipalityId, nisCode));
            return streetName;
        }

        public void ImportFromCrab(
            CrabStreetNameId streetNameId,
            CrabMunicipalityId municipalityId,
            CrabStreetName primaryStreetName,
            CrabStreetName secondaryStreetName,
            CrabTransStreetName primaryTransStreetName,
            CrabTransStreetName secondaryTransStreetName,
            CrabLanguage? primaryLanguage,
            CrabLanguage? secondaryLanguage,
            CrabLifetime lifeTime,
            CrabTimestamp timestamp,
            CrabOperator beginOperator,
            CrabModification? beginModification,
            CrabOrganisation? beginOrganisation)
        {
            if (IsRemoved)
                throw new InvalidOperationException($"Cannot change removed street name for street name id {_streetNameId}/{streetNameId}");

            if (beginModification == CrabModification.Delete)
            {
                ApplyChange(new StreetNameWasRemoved(_streetNameId));
            }
            else
            {
                ApplyRetiredOrUnretire(lifeTime, beginModification);

                if (beginModification != CrabModification.Correction)
                {
                    ApplyNameChanges(primaryStreetName, secondaryStreetName);
                    ApplyLanguageChanges(primaryLanguage?.ToLanguage(), secondaryLanguage?.ToLanguage());
                }
                else
                {
                    ApplyNameCorrections(primaryStreetName, secondaryStreetName);
                    ApplyLanguageCorrections(primaryLanguage?.ToLanguage(), secondaryLanguage?.ToLanguage());
                }
            }

            AssignPersistentLocalId(new PersistentLocalId(streetNameId), new PersistentLocalIdAssignmentDate(timestamp));

            ApplyChange(new StreetNameWasImportedFromCrab(
                streetNameId,
                municipalityId,
                primaryStreetName,
                secondaryStreetName,
                primaryTransStreetName,
                secondaryTransStreetName,
                primaryLanguage,
                secondaryLanguage,
                timestamp,
                lifeTime,
                beginOperator,
                beginModification,
                beginOrganisation));
        }

        public void ImportStatusFromCrab(
            CrabStreetNameStatusId streetNameStatusId,
            CrabStreetNameId streetNameId,
            CrabStreetNameStatus streetNameStatus,
            CrabLifetime lifeTime,
            CrabTimestamp timestamp,
            CrabOperator beginOperator,
            CrabModification? beginModification,
            CrabOrganisation? beginOrganisation)
        {
            if (IsRemoved && beginModification != CrabModification.Delete)
                throw new InvalidOperationException($"Cannot change removed street name for street name id {_streetNameId}/{streetNameId}");

            var legacyEvent = new StreetNameStatusWasImportedFromCrab(
                streetNameStatusId,
                streetNameId,
                streetNameStatus,
                lifeTime,
                timestamp,
                beginOperator,
                beginModification,
                beginOrganisation);

            ApplyStatusChangesFor(legacyEvent);

            ApplyCompletion();

            ApplyChange(legacyEvent);
        }

        public void AssignPersistentLocalId(
            PersistentLocalId persistentLocalId,
            PersistentLocalIdAssignmentDate assignmentDate)
        {
            if (_persistentLocalId != null)
                return;

            ApplyChange(new StreetNamePersistentLocalIdWasAssigned(
                _streetNameId,
                persistentLocalId,
                assignmentDate));
        }

        private void ApplyNameChanges(CrabStreetName primaryStreetName, CrabStreetName secondaryStreetName)
        {
            if (primaryStreetName != null && (primaryStreetName.Language != null || !string.IsNullOrEmpty(primaryStreetName.Name)))
                ApplyNameChange(primaryStreetName);

            if (secondaryStreetName != null && (secondaryStreetName.Language != null || !string.IsNullOrEmpty(secondaryStreetName.Name)))
                ApplyNameChange(secondaryStreetName);
        }

        private void ApplyNameChange(CrabStreetName streetName)
        {
            if (TryParseHomonymAddition(streetName.Name, out var name, out var homonymAddition))
            {
                if (!_homonymAdditions.HasMatch(streetName.Language?.ToLanguage(), homonymAddition))
                    ApplyChange(new StreetNameHomonymAdditionWasDefined(
                        _streetNameId,
                        new StreetNameHomonymAddition(homonymAddition, streetName.Language?.ToLanguage())));
            }
            else if (_homonymAdditions.HasLanguage(streetName.Language?.ToLanguage()))
            {
                ApplyChange(new StreetNameHomonymAdditionWasCleared(_streetNameId, streetName.Language?.ToLanguage()));
            }

            if (!_names.HasMatch(streetName.Language?.ToLanguage(), streetName.Name))
            {
                if (_names.HasLanguage(streetName.Language?.ToLanguage()) && string.IsNullOrEmpty(name))
                    ApplyChange(new StreetNameNameWasCleared(_streetNameId, streetName.Language?.ToLanguage()));
                else
                    ApplyChange(new StreetNameWasNamed(_streetNameId, new StreetNameName(name, streetName.Language?.ToLanguage())));
            }
        }

        private void ApplyNameCorrections(CrabStreetName primaryStreetName, CrabStreetName secondaryStreetName)
        {
            if (primaryStreetName != null && (primaryStreetName.Language != null || !string.IsNullOrEmpty(primaryStreetName.Name)))
                ApplyNameCorrection(primaryStreetName);

            if (secondaryStreetName != null && (secondaryStreetName.Language != null || !string.IsNullOrEmpty(secondaryStreetName.Name)))
                ApplyNameCorrection(secondaryStreetName);
        }

        private void ApplyNameCorrection(CrabStreetName streetName)
        {
            if (TryParseHomonymAddition(streetName.Name, out var name, out var homonymAddition))
            {
                if (!_homonymAdditions.HasMatch(streetName.Language?.ToLanguage(), homonymAddition))
                    ApplyChange(
                        new StreetNameHomonymAdditionWasCorrected(
                            _streetNameId,
                            new StreetNameHomonymAddition(homonymAddition, streetName.Language?.ToLanguage())));
            }
            else if (_homonymAdditions.HasLanguage(streetName.Language?.ToLanguage()))
            {
                ApplyChange(new StreetNameHomonymAdditionWasCorrectedToCleared(_streetNameId, streetName.Language?.ToLanguage()));
            }

            if (!_names.HasMatch(streetName.Language?.ToLanguage(), streetName.Name))
            {
                if (_names.HasLanguage(streetName.Language?.ToLanguage()) && string.IsNullOrEmpty(name))
                    ApplyChange(new StreetNameNameWasCorrectedToCleared(_streetNameId, streetName.Language?.ToLanguage()));
                else
                    ApplyChange(new StreetNameNameWasCorrected(_streetNameId, new StreetNameName(name, streetName.Language?.ToLanguage())));
            }
        }

        private void ApplyLanguageChanges(Language? primaryLanguage, Language? secondaryLanguage)
        {
            if (primaryLanguage != _primaryLanguage)
            {
                if (primaryLanguage.HasValue)
                    ApplyChange(new StreetNamePrimaryLanguageWasDefined(_streetNameId, primaryLanguage.Value));
                else
                    ApplyChange(new StreetNamePrimaryLanguageWasCleared(_streetNameId));
            }

            if (secondaryLanguage != _secondaryLanguage)
            {
                if (secondaryLanguage.HasValue)
                    ApplyChange(new StreetNameSecondaryLanguageWasDefined(_streetNameId, secondaryLanguage.Value));
                else
                    ApplyChange(new StreetNameSecondaryLanguageWasCleared(_streetNameId));
            }
        }

        private void ApplyLanguageCorrections(Language? primaryLanguage, Language? secondaryLanguage)
        {
            if (primaryLanguage != _primaryLanguage)
            {
                if (primaryLanguage.HasValue)
                    ApplyChange(new StreetNamePrimaryLanguageWasCorrected(_streetNameId, primaryLanguage.Value));
                else
                    ApplyChange(new StreetNamePrimaryLanguageWasCorrectedToCleared(_streetNameId));
            }

            if (secondaryLanguage != _secondaryLanguage)
            {
                if (secondaryLanguage.HasValue)
                    ApplyChange(new StreetNameSecondaryLanguageWasCorrected(_streetNameId, secondaryLanguage.Value));
                else
                    ApplyChange(new StreetNameSecondaryLanguageWasCorrectedToCleared(_streetNameId));
            }
        }

        private void ApplyRetiredOrUnretire(CrabLifetime lifeTime, CrabModification? beginModification)
        {
            if (lifeTime.EndDateTime.HasValue && !IsRetired)
            {
                if (beginModification != CrabModification.Correction)
                {
                    ApplyChange(new StreetNameWasRetired(_streetNameId));
                }
                else
                {
                    ApplyChange(new StreetNameWasCorrectedToRetired(_streetNameId));
                }
            }

            if (!lifeTime.EndDateTime.HasValue && IsRetired)
            {
                var streetNameStatusWasImportedFromCrab = _statusChronicle.MostCurrent(null);
                var previousStatus = streetNameStatusWasImportedFromCrab == null
                    ? null
                    : Map(streetNameStatusWasImportedFromCrab.StreetNameStatus, streetNameStatusWasImportedFromCrab.Modification);

                if (beginModification == CrabModification.Correction)
                {
                    ApplyStatusCorrection(previousStatus);
                }
                else
                {
                    ApplyStatusChange(previousStatus);
                }
            }
        }

        private void ApplyStatusChangesFor(StreetNameStatusWasImportedFromCrab legacyEvent)
        {
            var crabStatus = _statusChronicle.MostCurrent(legacyEvent);
            var newStatus = crabStatus == null
                ? null
                : Map(crabStatus.StreetNameStatus, crabStatus.Modification);

            if (_status == newStatus || IsRetired)
                return;

            if (crabStatus?.Modification == CrabModification.Correction)
            {
                ApplyStatusCorrection(newStatus);
            }
            else
            {
                ApplyStatusChange(newStatus);
            }
        }

        private void ApplyStatusChange(StreetNameStatus? status)
        {
            switch (status)
            {
                case StreetNameStatus.Proposed:
                    ApplyChange(new StreetNameWasProposed(_streetNameId));
                    break;

                case StreetNameStatus.Retired:
                    ApplyChange(new StreetNameWasRetired(_streetNameId));
                    break;

                case StreetNameStatus.Current:
                    ApplyChange(new StreetNameBecameCurrent(_streetNameId));
                    break;

                case null:
                    ApplyChange(new StreetNameStatusWasRemoved(_streetNameId));
                    break;
            }
        }

        private void ApplyStatusCorrection(StreetNameStatus? status)
        {
            switch (status)
            {
                case StreetNameStatus.Proposed:
                    ApplyChange(new StreetNameWasCorrectedToProposed(_streetNameId));
                    break;

                case StreetNameStatus.Retired:
                    ApplyChange(new StreetNameWasCorrectedToRetired(_streetNameId));
                    break;

                case StreetNameStatus.Current:
                    ApplyChange(new StreetNameWasCorrectedToCurrent(_streetNameId));
                    break;

                case null:
                    ApplyChange(new StreetNameStatusWasCorrectedToRemoved(_streetNameId));
                    break;
            }
        }

        private void ApplyCompletion()
        {
            if (_status == null && _isCompleted)
            {
                ApplyChange(new StreetNameBecameIncomplete(_streetNameId));
            }
            else if (_status != null && !_isCompleted)
            {
                ApplyChange(new StreetNameBecameComplete(_streetNameId));
            }
        }

        private static StreetNameStatus? Map(CrabStreetNameStatus crabStreetNameStatus, CrabModification? modification)
        {
            if (modification == CrabModification.Delete)
                return null;

            switch (crabStreetNameStatus)
            {
                case CrabStreetNameStatus.Unknown:
                    return null;

                case CrabStreetNameStatus.Proposed:
                case CrabStreetNameStatus.Reserved:
                    return StreetNameStatus.Proposed;

                default:
                    return StreetNameStatus.Current;
            }
        }

        private static bool TryParseHomonymAddition(string input, out string streetName, out string homonym)
        {
            if (input == null)
            {
                streetName = null;
                homonym = null;
                return false;
            }

            var parts = input.Split('_');

            if (parts.Length > 1 && parts.Last().Length > 0)
            {
                streetName = string.Join("_", parts.Take(parts.Length - 1));
                homonym = parts.Last();
                return true;
            }

            homonym = null;
            streetName = input;
            return false;
        }
    }
}
