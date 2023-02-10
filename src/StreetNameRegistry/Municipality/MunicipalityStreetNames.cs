namespace StreetNameRegistry.Municipality
{
    using System.Collections.Generic;
    using System.Linq;
    using Exceptions;

    public sealed class MunicipalityStreetNames : List<MunicipalityStreetName>
    {
        public bool HasPersistentLocalId(PersistentLocalId persistentLocalId)
            => this.Any(x => x.PersistentLocalId == persistentLocalId);

        public bool HasActiveStreetNameName(StreetNameName streetNameName, HomonymAdditions homonymAdditions, PersistentLocalId existingStreetNamePersistentLocalId)
        {
            var homonymAddition = homonymAdditions.SingleOrDefault(x => x.Language == streetNameName.Language);

            return this.Any(x =>
                    x.PersistentLocalId != existingStreetNamePersistentLocalId
                    && !x.IsRemoved
                    && !x.IsRetired
                    && !x.IsRejected
                    && x.Names.HasMatch(streetNameName.Language, streetNameName.Name)
                    && HasHomonymAdditionMatch(x, streetNameName, homonymAddition));
        }

        public MunicipalityStreetName FindByPersistentLocalId(PersistentLocalId persistentLocalId)
        {
            var streetName = this.SingleOrDefault(x => x.PersistentLocalId == persistentLocalId);

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
            => this.Single(x => x.PersistentLocalId == persistentLocalId);

        private static bool HasHomonymAdditionMatch(MunicipalityStreetName possibleDuplicateStreetName, StreetNameName streetNameName, StreetNameHomonymAddition? homonymAddition)
        {
            return !possibleDuplicateStreetName.HomonymAdditions.HasLanguage(streetNameName.Language) && homonymAddition is null
                   || homonymAddition is not null && possibleDuplicateStreetName.HomonymAdditions.HasMatch(homonymAddition.Language, homonymAddition.HomonymAddition);
        }
    }
}
