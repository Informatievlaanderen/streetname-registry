namespace StreetNameRegistry.Municipality.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class RemoveMunicipality : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("678dd71f-cc41-41f0-8b81-0e9829965269");

        public MunicipalityId MunicipalityId { get; }
        public Provenance Provenance { get; }

        public RemoveMunicipality(
            MunicipalityId municipalityId,
            Provenance provenance)
        {
            MunicipalityId = municipalityId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"RemoveMunicipality-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return MunicipalityId;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
