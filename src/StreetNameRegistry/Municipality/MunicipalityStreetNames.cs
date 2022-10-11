namespace StreetNameRegistry.Municipality
{
    using System.Collections.Generic;
    using System.Linq;
    using Exceptions;

    public sealed class MunicipalityStreetNames : List<MunicipalityStreetName>
    {
        public bool HasPersistentLocalId(PersistentLocalId persistentLocalId)
            => this.Any(x => x.PersistentLocalId == persistentLocalId);

        public bool HasActiveStreetNameName(StreetNameName streetNameName, PersistentLocalId existingStreetNamePersistentLocalId)
            => this.Any(x => !x.IsRemoved
                             && !x.IsRetired
                             && !x.IsRejected
                             && x.Names.HasMatch(streetNameName.Language, streetNameName.Name)
                             && x.PersistentLocalId != existingStreetNamePersistentLocalId);

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
    }
}
