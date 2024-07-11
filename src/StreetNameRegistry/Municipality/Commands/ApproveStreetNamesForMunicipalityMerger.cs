namespace StreetNameRegistry.Municipality.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class ApproveStreetNamesForMunicipalityMerger : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("926945a5-1f96-4ed6-9ddc-d27a588de3ee");

        public MunicipalityId MunicipalityId { get; }

        public Provenance Provenance { get; }

        public ApproveStreetNamesForMunicipalityMerger(
            MunicipalityId municipalityId,
            Provenance provenance)
        {
            MunicipalityId = municipalityId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ApproveStreetNamesForMunicipalityMerger-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return MunicipalityId;

            foreach (var field in Provenance.GetIdentityFields())
                yield return field;
        }
    }
}
