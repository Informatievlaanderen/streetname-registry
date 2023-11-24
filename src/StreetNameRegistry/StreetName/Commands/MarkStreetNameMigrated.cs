namespace StreetNameRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    [Obsolete("This is a legacy command and should not be used anymore.")]
    public sealed class MarkStreetNameMigrated : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("b9233165-3c9f-4668-917d-375df3086842");

        public MunicipalityId MunicipalityId { get; }
        public StreetNameId StreetNameId { get; }
        public Provenance Provenance { get; }

        public MarkStreetNameMigrated(
            MunicipalityId municipalityId,
            StreetNameId streetNameId,
            Provenance provenance)
        {
            MunicipalityId = municipalityId;
            StreetNameId = streetNameId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"MarkStreetNameMigrated-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return StreetNameId;
            yield return MunicipalityId;
        }
    }
}
