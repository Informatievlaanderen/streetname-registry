namespace StreetNameRegistry.Municipality
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Exceptions;

    public sealed class MunicipalityStreetNames : IEnumerable<MunicipalityStreetName>
    {
        private readonly List<MunicipalityStreetName> _streetNames;
        private readonly Dictionary<PersistentLocalId, MunicipalityStreetName> _streetNamesByPersistentLocalId;

        public MunicipalityStreetNames()
        {
            _streetNames = new List<MunicipalityStreetName>();
            _streetNamesByPersistentLocalId = new Dictionary<PersistentLocalId, MunicipalityStreetName>();
        }

        public bool HasPersistentLocalId(PersistentLocalId persistentLocalId)
            => _streetNamesByPersistentLocalId.ContainsKey(persistentLocalId);

        public bool HasActiveStreetNameName(StreetNameName streetNameName, HomonymAdditions homonymAdditions, PersistentLocalId existingStreetNamePersistentLocalId)
        {
            var homonymAddition = homonymAdditions.SingleOrDefault(x => x.Language == streetNameName.Language);

            return _streetNames.Any(x =>
                    x.PersistentLocalId != existingStreetNamePersistentLocalId
                    && !x.IsRemoved
                    && !x.IsRetired
                    && !x.IsRejected
                    && x.Names.HasMatch(streetNameName.Language, streetNameName.Name)
                    && HasHomonymAdditionMatch(x, streetNameName, homonymAddition));
        }

        public MunicipalityStreetName FindByPersistentLocalId(PersistentLocalId persistentLocalId)
        {
            _streetNamesByPersistentLocalId.TryGetValue(persistentLocalId, out var streetName);

            if (streetName is null)
            {
                throw new StreetNameIsNotFoundException(persistentLocalId);
            }

            return streetName;
        }

        public MunicipalityStreetName GetNotRemovedByPersistentLocalId(PersistentLocalId persistentLocalId)
        {
            var streetName = FindByPersistentLocalId(persistentLocalId);

            if (streetName.IsRemoved)
            {
                throw new StreetNameIsRemovedException(persistentLocalId);
            }

            return streetName;
        }

        public MunicipalityStreetName GetByPersistentLocalId(PersistentLocalId persistentLocalId)
            => _streetNamesByPersistentLocalId[persistentLocalId];

        private static bool HasHomonymAdditionMatch(MunicipalityStreetName possibleDuplicateStreetName, StreetNameName streetNameName, StreetNameHomonymAddition? homonymAddition)
        {
            return !possibleDuplicateStreetName.HomonymAdditions.HasLanguage(streetNameName.Language) && homonymAddition is null
                   || homonymAddition is not null && possibleDuplicateStreetName.HomonymAdditions.HasMatch(homonymAddition.Language, homonymAddition.HomonymAddition);
        }

        public void Add(MunicipalityStreetName streetName)
        {
            _streetNames.Add(streetName);
            _streetNamesByPersistentLocalId.Add(streetName.PersistentLocalId, streetName);
        }

        public IEnumerator<MunicipalityStreetName> GetEnumerator()
        {
            return _streetNames.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
