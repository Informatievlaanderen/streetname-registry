namespace StreetNameRegistry.Municipality
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Events;
    using Exceptions;

    public sealed partial class Municipality : AggregateRootEntity, ISnapshotable
    {
        public static Municipality Register(
            IMunicipalityFactory municipalityFactory,
            MunicipalityId municipalityId,
            NisCode nisCode)
        {
            var municipality = municipalityFactory.Create();
            municipality.ApplyChange(new MunicipalityWasImported(municipalityId, nisCode));
            return municipality;
        }

        public void ProposeStreetName(Names streetNameNames, PersistentLocalId persistentLocalId)
        {
            if (MunicipalityStatus == MunicipalityStatus.Retired)
            {
                throw new MunicipalityHasInvalidStatusException($"Municipality with id '{_municipalityId}' was retired");
            }

            if (StreetNames.HasPersistentLocalId(persistentLocalId))
            {
                throw new StreetNamePersistentLocalIdAlreadyExistsException();
            }

            GuardStreetNameNames(streetNameNames, persistentLocalId);

            foreach (var language in _officialLanguages.Concat(_facilityLanguages))
            {
                if (!streetNameNames.HasLanguage(language))
                {
                    throw new StreetNameIsMissingALanguageException($"The language '{language}' is missing.");
                }
            }

            ApplyChange(new StreetNameWasProposedV2(_municipalityId, _nisCode, streetNameNames, persistentLocalId));
        }

        public void ApproveStreetName(PersistentLocalId persistentLocalId)
        {
            var streetName = StreetNames.GetNotRemovedByPersistentLocalId(persistentLocalId);

            if (MunicipalityStatus != MunicipalityStatus.Current)
            {
                throw new MunicipalityHasInvalidStatusException();
            }

            streetName.Approve();
        }

        public void CorrectStreetNameApproval(PersistentLocalId persistentLocalId)
        {
            var streetName = StreetNames.GetNotRemovedByPersistentLocalId(persistentLocalId);

            if (MunicipalityStatus != MunicipalityStatus.Current)
            {
                throw new MunicipalityHasInvalidStatusException();
            }

            streetName.CorrectApproval();
        }

        public void RejectStreetName(PersistentLocalId persistentLocalId)
        {
            var streetName = StreetNames.GetNotRemovedByPersistentLocalId(persistentLocalId);

            if (MunicipalityStatus != MunicipalityStatus.Current)
            {
                throw new MunicipalityHasInvalidStatusException();
            }

            streetName.Reject();
        }

        public void CorrectStreetNameRejection(PersistentLocalId persistentLocalId)
        {
            var streetName = StreetNames.GetNotRemovedByPersistentLocalId(persistentLocalId);

            if (MunicipalityStatus != MunicipalityStatus.Current)
            {
                throw new MunicipalityHasInvalidStatusException();
            }

            streetName.CorrectRejection(() => GuardUniqueActiveStreetNameNames(streetName.Names, persistentLocalId));
        }

        public void RetireStreetName(PersistentLocalId persistentLocalId)
        {
            var streetName = StreetNames.GetNotRemovedByPersistentLocalId(persistentLocalId);

            if (MunicipalityStatus != MunicipalityStatus.Current)
            {
                throw new MunicipalityHasInvalidStatusException();
            }

            streetName.Retire();
        }

        public void CorrectStreetNameRetirement(PersistentLocalId persistentLocalId)
        {
            var streetName = StreetNames.GetNotRemovedByPersistentLocalId(persistentLocalId);

            if (MunicipalityStatus != MunicipalityStatus.Current)
            {
                throw new MunicipalityHasInvalidStatusException();
            }

            streetName.CorrectRetirement(() => GuardUniqueActiveStreetNameNames(streetName.Names, persistentLocalId));
        }

        private void GuardUniqueActiveStreetNameNames(Names streetNameNames, PersistentLocalId persistentLocalId)
        {
            foreach (var streetNameName in streetNameNames.Where(streetNameName => StreetNames.HasActiveStreetNameName(streetNameName, persistentLocalId)))
            {
                throw new StreetNameNameAlreadyExistsException(streetNameName.Name);
            }
        }

        public void CorrectStreetNameName(Names streetNameNames, PersistentLocalId persistentLocalId)
        {
            StreetNames
                .GetNotRemovedByPersistentLocalId(persistentLocalId)
                .CorrectNames(streetNameNames, GuardStreetNameNames);
        }

        private void GuardStreetNameNames(Names streetNameNames, PersistentLocalId persistentLocalId)
        {
            GuardUniqueActiveStreetNameNames(streetNameNames, persistentLocalId);

            foreach (var language in streetNameNames.Select(x => x.Language))
            {
                if (!_officialLanguages.Contains(language) && !_facilityLanguages.Contains(language))
                {
                    throw new StreetNameNameLanguageIsNotSupportedException("The language '{language}' is not an official or facility language of municipality '{_municipalityId}'.");
                }
            }
        }

        public void DefineOrChangeNisCode(NisCode nisCode)
        {
            if (string.IsNullOrWhiteSpace(nisCode))
            {
                throw new NoNisCodeHasNoValueException("NisCode of a municipality cannot be empty.");
            }

            if (nisCode != _nisCode)
            {
                ApplyChange(new MunicipalityNisCodeWasChanged(_municipalityId, nisCode));
            }
        }

        public void BecomeCurrent()
        {
            if (MunicipalityStatus != MunicipalityStatus.Current)
            {
                ApplyChange(new MunicipalityBecameCurrent(_municipalityId));
            }
        }

        public void CorrectToCurrent()
        {
            if (MunicipalityStatus != MunicipalityStatus.Current)
            {
                ApplyChange(new MunicipalityWasCorrectedToCurrent(_municipalityId));
            }
        }

        public void CorrectToRetired()
        {
            if (MunicipalityStatus != MunicipalityStatus.Retired)
            {
                ApplyChange(new MunicipalityWasCorrectedToRetired(_municipalityId));
            }
        }

        public void Retire()
        {
            if (MunicipalityStatus != MunicipalityStatus.Retired)
            {
                ApplyChange(new MunicipalityWasRetired(_municipalityId));
            }
        }

        public void NameMunicipality(MunicipalityName name)
        {
            ApplyChange(new MunicipalityWasNamed(_municipalityId, name));
        }

        public void AddOfficialLanguage(Language language)
        {
            if (!_officialLanguages.Contains(language))
            {
                ApplyChange(new MunicipalityOfficialLanguageWasAdded(_municipalityId, language));
            }
        }

        public void RemoveOfficialLanguage(Language language)
        {
            if (_officialLanguages.Contains(language))
            {
                ApplyChange(new MunicipalityOfficialLanguageWasRemoved(_municipalityId, language));
            }
        }

        public void AddFacilityLanguage(Language language)
        {
            if (!_facilityLanguages.Contains(language))
            {
                ApplyChange(new MunicipalityFacilityLanguageWasAdded(_municipalityId, language));
            }
        }

        public void RemoveFacilityLanguage(Language language)
        {
            if (_facilityLanguages.Contains(language))
            {
                ApplyChange(new MunicipalityFacilityLanguageWasRemoved(_municipalityId, language));
            }
        }

        public void MigrateStreetName(
            StreetNameId streetNameId,
            PersistentLocalId persistentLocalId,
            StreetNameStatus status,
            Language? primaryLanguage,
            Language? secondaryLanguage,
            Names names,
            HomonymAdditions homonymAdditions,
            bool isCompleted,
            bool isRemoved)
        {
            if (StreetNames.HasPersistentLocalId(persistentLocalId))
            {
                throw new InvalidOperationException(
                    $"Cannot migrate StreetName with id '{persistentLocalId}' in municipality '{_municipalityId}'.");
            }

            ApplyChange(new StreetNameWasMigratedToMunicipality(
                _municipalityId,
                _nisCode,
                streetNameId,
                persistentLocalId,
                status,
                primaryLanguage,
                secondaryLanguage,
                names,
                homonymAdditions,
                isCompleted,
                isRemoved));
        }

        public string GetStreetNameHash(PersistentLocalId persistentLocalId)
        {
            var streetName = StreetNames.FindByPersistentLocalId(persistentLocalId);

            return streetName.LastEventHash;
        }

        #region Metadata
        protected override void BeforeApplyChange(object @event)
        {
            _ = new EventMetadataContext(new Dictionary<string, object>());
            base.BeforeApplyChange(@event);
        }

        #endregion

        public object TakeSnapshot()
        {
            return new MunicipalitySnapshot(
                MunicipalityId,
                _nisCode,
                MunicipalityStatus,
                _officialLanguages,
                _facilityLanguages,
                StreetNames);
        }

        public ISnapshotStrategy Strategy { get; }
    }
}
