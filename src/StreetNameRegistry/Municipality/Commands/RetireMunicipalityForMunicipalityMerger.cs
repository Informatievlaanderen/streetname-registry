namespace StreetNameRegistry.Municipality.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class RetireMunicipalityForMunicipalityMerger : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("5a75882a-ada4-44d5-9a90-71dc3b0fe6b9");

        public MunicipalityId MunicipalityId { get; }
        public MunicipalityId NewMunicipalityId { get; }

        public Provenance Provenance { get; }

        public RetireMunicipalityForMunicipalityMerger(
            MunicipalityId municipalityId,
            MunicipalityId newMunicipalityId,
            Provenance provenance)
        {
            MunicipalityId = municipalityId;
            NewMunicipalityId = newMunicipalityId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"RetireMunicipalityForMunicipalityMerger-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return MunicipalityId;
            yield return NewMunicipalityId;
        }
    }
}
