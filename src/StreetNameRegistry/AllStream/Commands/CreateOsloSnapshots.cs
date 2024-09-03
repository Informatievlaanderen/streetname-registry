namespace StreetNameRegistry.AllStream.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Municipality;

    public sealed class CreateOsloSnapshots : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("449dfd1c-4af4-44a0-9a19-85359eba972d");

        public IReadOnlyList<PersistentLocalId> PersistentLocalIds { get; }

        public Provenance Provenance { get; }

        public CreateOsloSnapshots(
            IEnumerable<PersistentLocalId> persistentLocalIds,
            Provenance provenance)
        {
            PersistentLocalIds = persistentLocalIds.ToList();
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"CreateOsloSnapshots-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return PersistentLocalIds;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
