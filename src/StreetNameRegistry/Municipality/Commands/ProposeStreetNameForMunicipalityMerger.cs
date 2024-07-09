namespace StreetNameRegistry.Municipality.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class ProposeStreetNameForMunicipalityMerger : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("eaf63006-10de-403f-a692-084707cc5ed4");

        public MunicipalityId MunicipalityId { get; }
        public Names StreetNameNames { get; }
        public HomonymAdditions HomonymAdditions { get; }
        public PersistentLocalId PersistentLocalId { get; }

        public List<MunicipalityId> MergedMunicipalityIds { get; }
        public List<PersistentLocalId> MergedStreetNamePersistentLocalIds { get; }
        public Provenance Provenance { get; }
        public ProposeStreetNameForMunicipalityMerger(
            MunicipalityId municipalityId,
            Names streetNameNames,
            HomonymAdditions? homonymAdditions,
            PersistentLocalId persistentLocalId,
            List<MunicipalityId> mergedMunicipalityIds,
            List<PersistentLocalId> mergedStreetNamePersistentLocalIds,
            Provenance provenance)
        {
            MunicipalityId = municipalityId;
            StreetNameNames = streetNameNames;
            HomonymAdditions = homonymAdditions ?? [];
            PersistentLocalId = persistentLocalId;
            MergedMunicipalityIds = mergedMunicipalityIds;
            MergedStreetNamePersistentLocalIds = mergedStreetNamePersistentLocalIds;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ProposeStreetNameForMunicipalityMerger-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return MunicipalityId;
            yield return PersistentLocalId;

            foreach (var streetNameName in StreetNameNames)
            {
                yield return streetNameName;
            }

            foreach (var homonymAddition in HomonymAdditions)
            {
                yield return homonymAddition;
            }

            foreach (var mergedMunicipalityId in MergedMunicipalityIds)
            {
                yield return mergedMunicipalityId;
            }

            foreach (var mergedStreetNamePersistentLocalId in MergedStreetNamePersistentLocalIds)
            {
                yield return mergedStreetNamePersistentLocalId;
            }
        }
    }
}
