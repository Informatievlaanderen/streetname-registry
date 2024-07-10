namespace StreetNameRegistry.Municipality.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class RetireStreetNamesForMunicipalityMerger : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("5a75882a-ada4-44d5-9a90-71dc3b0fe6b9");

        public MunicipalityId MunicipalityId { get; }

        public Provenance Provenance { get; }

        public RetireStreetNamesForMunicipalityMerger(
            MunicipalityId municipalityId,
            Provenance provenance)
        {
            MunicipalityId = municipalityId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"RetireStreetNamesForMunicipalityMerger-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return MunicipalityId;
            //TODO-rik mss toch lijst van streetnameids meegeven, wat als er later nog een merge gebeurd naar dezelfde gemeente?
            //voor idempotency, of provenance hiervoor gebruiken met timestamp?
        }
    }
}
