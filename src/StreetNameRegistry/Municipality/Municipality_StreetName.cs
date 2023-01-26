namespace StreetNameRegistry.Municipality
{
    using System;
    using System.Linq;
    using Events;
    using Exceptions;

    public partial class Municipality
    {
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

        public void RejectStreetName(PersistentLocalId persistentLocalId)
        {
            var streetName = StreetNames.GetNotRemovedByPersistentLocalId(persistentLocalId);

            if (MunicipalityStatus != MunicipalityStatus.Current)
            {
                throw new MunicipalityHasInvalidStatusException();
            }

            streetName.Reject();
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

        public void CorrectStreetNameName(Names streetNameNames, PersistentLocalId persistentLocalId)
        {
            StreetNames
                .GetNotRemovedByPersistentLocalId(persistentLocalId)
                .CorrectNames(streetNameNames, GuardStreetNameNames);
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

        public void CorrectStreetNameRejection(PersistentLocalId persistentLocalId)
        {
            var streetName = StreetNames.GetNotRemovedByPersistentLocalId(persistentLocalId);

            if (MunicipalityStatus != MunicipalityStatus.Current)
            {
                throw new MunicipalityHasInvalidStatusException();
            }

            streetName.CorrectRejection(() => GuardUniqueActiveStreetNameNames(streetName.Names, persistentLocalId));
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
            var namesWithActiveStreetNameName = streetNameNames
                .Where(streetNameName => StreetNames.HasActiveStreetNameName(streetNameName, persistentLocalId))
                .Select(x => x.Name);

            if (namesWithActiveStreetNameName.Any())
            {
                var firstName = streetNameNames.FirstOrDefault();
                if (firstName is not null)
                {
                    throw new StreetNameNameAlreadyExistsException(firstName.Name);
                }
            }
        }

        private void GuardStreetNameNames(Names streetNameNames, PersistentLocalId persistentLocalId)
        {
            GuardUniqueActiveStreetNameNames(streetNameNames, persistentLocalId);

            foreach (var language in streetNameNames.Select(x => x.Language))
            {
                if (!_officialLanguages.Contains(language) && !_facilityLanguages.Contains(language))
                {
                    throw new StreetNameNameLanguageIsNotSupportedException($"The language '{language}' is not an official or facility language of municipality '{_municipalityId}'.");
                }
            }
        }

        public string GetStreetNameHash(PersistentLocalId persistentLocalId)
        {
            var streetName = StreetNames.FindByPersistentLocalId(persistentLocalId);

            return streetName.LastEventHash;
        }
    }
}
