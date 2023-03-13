namespace StreetNameRegistry.Municipality.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class ChangeStreetNameNames : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("831e6864-f78a-498d-b3b3-8c6e78435098");

        public MunicipalityId MunicipalityId { get; }
        public PersistentLocalId PersistentLocalId { get; }
        public Provenance Provenance { get; }
        public Names StreetNameNames { get; }

        public ChangeStreetNameNames(MunicipalityId municipalityId,
            PersistentLocalId persistentLocalId,
            Names streetNameNames,
            Provenance provenance)
        {
            MunicipalityId = municipalityId;
            PersistentLocalId = persistentLocalId;
            StreetNameNames = streetNameNames;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ChangeStreetNameNames-{ToString()}");

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

            foreach (var streetNameName in StreetNameNames)
            {
                yield return streetNameName;
            }
        }
    }
}
