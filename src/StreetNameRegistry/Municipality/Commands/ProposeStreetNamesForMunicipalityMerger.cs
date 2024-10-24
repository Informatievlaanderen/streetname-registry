namespace StreetNameRegistry.Municipality.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class ProposeStreetNamesForMunicipalityMerger : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("eaf63006-10de-403f-a692-084707cc5ed4");

        public MunicipalityId MunicipalityId { get; }
        public List<StreetNameToPropose> StreetNames { get; }
        public Provenance Provenance { get; }

        public ProposeStreetNamesForMunicipalityMerger(
            MunicipalityId municipalityId,
            List<StreetNameToPropose> streetNames,
            Provenance provenance)
        {
            MunicipalityId = municipalityId;
            StreetNames = streetNames;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ProposeStreetNamesForMunicipalityMerger-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return MunicipalityId;

            foreach (var streetNameName in StreetNames.SelectMany(x => x.IdentityFields()))
            {
                yield return streetNameName;
            }

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }

        public sealed class StreetNameToPropose
        {
            public StreetNameStatus DesiredStatus { get; }
            public Names StreetNameNames { get; }
            public HomonymAdditions HomonymAdditions { get; }
            public PersistentLocalId PersistentLocalId { get; }
            public List<PersistentLocalId> MergedStreetNamePersistentLocalIds { get; }

            public StreetNameToPropose(
                StreetNameStatus desiredStatus,
                Names streetNameNames,
                HomonymAdditions homonymAdditions,
                PersistentLocalId persistentLocalId,
                List<PersistentLocalId> mergedStreetNamePersistentLocalIds)
            {
                DesiredStatus = desiredStatus;
                StreetNameNames = streetNameNames;
                HomonymAdditions = homonymAdditions;
                PersistentLocalId = persistentLocalId;
                MergedStreetNamePersistentLocalIds = mergedStreetNamePersistentLocalIds;
            }

            internal IEnumerable<object> IdentityFields()
            {
                yield return PersistentLocalId;
                yield return DesiredStatus;

                foreach (var streetNameName in StreetNameNames)
                {
                    yield return streetNameName;
                }

                foreach (var homonymAddition in HomonymAdditions)
                {
                    yield return homonymAddition;
                }

                foreach (var mergedStreetNamePersistentLocalId in MergedStreetNamePersistentLocalIds)
                {
                    yield return mergedStreetNamePersistentLocalId;
                }
            }
        }
    }
}
