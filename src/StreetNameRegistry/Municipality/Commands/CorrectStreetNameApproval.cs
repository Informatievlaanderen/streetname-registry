namespace StreetNameRegistry.Municipality.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class CorrectStreetNameApproval : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("36c01885-1f0b-48e2-b636-b238f100c9ab");

        public MunicipalityId MunicipalityId { get; }
        public PersistentLocalId PersistentLocalId { get; }
        public Provenance Provenance { get; }

        public CorrectStreetNameApproval(
            MunicipalityId municipalityId,
            PersistentLocalId persistentLocalId,
            Provenance provenance)
        {
            MunicipalityId = municipalityId;
            Provenance = provenance;
            PersistentLocalId = persistentLocalId;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"CorrectStreetNameApproval-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return MunicipalityId;
            yield return PersistentLocalId;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
