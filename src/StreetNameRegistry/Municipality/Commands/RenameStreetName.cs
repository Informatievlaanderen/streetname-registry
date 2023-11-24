namespace StreetNameRegistry.Municipality.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class RenameStreetName : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("156b0793-cf0e-42e5-8bfc-9275cc736f63");

        public MunicipalityId MunicipalityId { get; }
        public Provenance Provenance { get; }
        public PersistentLocalId PersistentLocalId { get; }
        public PersistentLocalId DestinationPersistentLocalId { get; }

        public RenameStreetName(
            MunicipalityId municipalityId,
            PersistentLocalId persistentLocalId,
            PersistentLocalId destinationPersistentLocalId,
            Provenance provenance)
        {
            MunicipalityId = municipalityId;
            PersistentLocalId = persistentLocalId;
            DestinationPersistentLocalId = destinationPersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"RenameStreetName-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return MunicipalityId;

            yield return PersistentLocalId;
            
            yield return DestinationPersistentLocalId;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
