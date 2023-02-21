namespace StreetNameRegistry.Municipality.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class CorrectMunicipalityName : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("85068206-cc11-401b-8472-077ee15e8ac3");

        public MunicipalityId MunicipalityId { get; }
        public MunicipalityName Name { get; }
        public Provenance Provenance { get; }

        public CorrectMunicipalityName(MunicipalityId municipalityId, MunicipalityName name, Provenance provenance)
        {
            MunicipalityId = municipalityId;
            Name = name;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"{nameof(CorrectMunicipalityName)}-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return MunicipalityId;
            yield return Name;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
