namespace StreetNameRegistry.Tests.AggregateTests.Extensions
{
    using System.Collections.Generic;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Commands;

    public static class ProposeStreetNameForMunicipalityMergerExtensions
    {
        public static ProposeStreetNameForMunicipalityMerger WithMunicipalityId(this ProposeStreetNameForMunicipalityMerger command, MunicipalityId municipalityId)
        {
            return new ProposeStreetNameForMunicipalityMerger(
                municipalityId,
                command.StreetNameNames,
                command.HomonymAdditions,
                command.PersistentLocalId,
                command.MergedStreetNamePersistentLocalIds,
                command.Provenance);
        }

        public static ProposeStreetNameForMunicipalityMerger WithRandomStreetName(this ProposeStreetNameForMunicipalityMerger command, Fixture fixture)
        {
            return new ProposeStreetNameForMunicipalityMerger(
                command.MunicipalityId,
                new Names(new List<StreetNameName> { fixture.Create<StreetNameName>() }),
                command.HomonymAdditions,
                command.PersistentLocalId,
                command.MergedStreetNamePersistentLocalIds,
                command.Provenance);
        }

        public static ProposeStreetNameForMunicipalityMerger WithStreetNameNames(this ProposeStreetNameForMunicipalityMerger command, Names names)
        {
            return new ProposeStreetNameForMunicipalityMerger(
                command.MunicipalityId,
                names,
                command.HomonymAdditions,
                command.PersistentLocalId,
                command.MergedStreetNamePersistentLocalIds,
                command.Provenance);
        }

        public static ProposeStreetNameForMunicipalityMerger WithHomonymAdditions(this ProposeStreetNameForMunicipalityMerger command, HomonymAdditions homonymAdditions)
        {
            return new ProposeStreetNameForMunicipalityMerger(
                command.MunicipalityId,
                command.StreetNameNames,
                homonymAdditions,
                command.PersistentLocalId,
                command.MergedStreetNamePersistentLocalIds,
                command.Provenance);
        }

        public static ProposeStreetNameForMunicipalityMerger WithPersistentLocalId(this ProposeStreetNameForMunicipalityMerger command, PersistentLocalId persistentLocalId)
        {
            return new ProposeStreetNameForMunicipalityMerger(
                command.MunicipalityId,
                command.StreetNameNames,
                command.HomonymAdditions,
                persistentLocalId,
                command.MergedStreetNamePersistentLocalIds,
                command.Provenance);
        }

        public static ProposeStreetNameForMunicipalityMerger WithMergedStreetNamePersistentIds(this ProposeStreetNameForMunicipalityMerger command, List<PersistentLocalId> mergedStreetNamePersistentIds)
        {
            return new ProposeStreetNameForMunicipalityMerger(
                command.MunicipalityId,
                command.StreetNameNames,
                command.HomonymAdditions,
                command.PersistentLocalId,
                mergedStreetNamePersistentIds,
                command.Provenance);
        }
    }
}
