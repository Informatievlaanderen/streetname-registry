namespace StreetNameRegistry.Municipality.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class ProposeStreetName : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("55378fee-18e5-4e26-abd0-36692639a146");

        public MunicipalityId MunicipalityId { get; }
        public Provenance Provenance { get; }
        public Names StreetNameNames { get; }
        public PersistentLocalId PersistentLocalId { get; }

        public ProposeStreetName(
            MunicipalityId municipalityId,
            Names streetNameNames,
            PersistentLocalId persistentLocalId,
            Provenance provenance)
        {
            MunicipalityId = municipalityId;
            StreetNameNames = streetNameNames;
            Provenance = provenance;
            PersistentLocalId = persistentLocalId;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ProposeStreetName-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return MunicipalityId;

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
