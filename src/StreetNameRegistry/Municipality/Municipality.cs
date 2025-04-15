namespace StreetNameRegistry.Municipality
{
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

        public void DefineOrChangeNisCode(NisCode nisCode)
        {
            if (nisCode is null || string.IsNullOrWhiteSpace(nisCode))
            {
                throw new NoNisCodeHasNoValueException("NisCode of a municipality cannot be empty.");
            }

            if (nisCode != _nisCode)
            {
                ApplyChange(new MunicipalityNisCodeWasChanged(_municipalityId, nisCode, StreetNames.Select(x => x.PersistentLocalId).ToList()));
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

        public void BecomeCurrent()
        {
            if (MunicipalityStatus != MunicipalityStatus.Current)
            {
                ApplyChange(new MunicipalityBecameCurrent(_municipalityId));
            }
        }

        public void Retire()
        {
            if (StreetNames.Any(x => x.Status is StreetNameStatus.Current or StreetNameStatus.Proposed && !x.IsRemoved))
                throw new StreetNamesAreActiveException();

            if (MunicipalityStatus != MunicipalityStatus.Retired)
            {
                ApplyChange(new MunicipalityWasRetired(_municipalityId));
            }
        }

        public void RetireForMunicipalityMerger(Municipality newMunicipality)
        {
            var currentStreetNames = StreetNames
                .Where(x => x.Status == StreetNameStatus.Current && !x.IsRemoved)
                .ToList();

            foreach (var streetName in currentStreetNames)
            {
                var newStreetNamePersistentLocalIds = newMunicipality.StreetNames
                    .Where(x =>
                        !x.IsRemoved &&
                        x.MergedStreetNamePersistentLocalIds.Contains(streetName.PersistentLocalId))
                    .Select(x => x.PersistentLocalId)
                    .ToList();

                streetName.RetireForMunicipalityMerger(newStreetNamePersistentLocalIds);
            }

            var proposedStreetNames = StreetNames
                .Where(x => x.Status == StreetNameStatus.Proposed && !x.IsRemoved)
                .ToList();

            foreach (var streetName in proposedStreetNames)
            {
                var newStreetNamePersistentLocalIds = newMunicipality.StreetNames
                    .Where(x =>
                        !x.IsRemoved &&
                        x.MergedStreetNamePersistentLocalIds.Contains(streetName.PersistentLocalId))
                    .Select(x => x.PersistentLocalId)
                    .ToList();

                streetName.RejectForMunicipalityMerger(newStreetNamePersistentLocalIds);
            }

            if (MunicipalityStatus != MunicipalityStatus.Retired)
            {
                ApplyChange(new MunicipalityWasMerged(_municipalityId, newMunicipality.MunicipalityId));
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
