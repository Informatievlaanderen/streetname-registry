namespace StreetNameRegistry.Tests.AggregateTests.Extensions
{
    using System.Collections.Generic;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Commands;
    using StreetNameToPropose = Municipality.Commands.ProposeStreetNamesForMunicipalityMerger.StreetNameToPropose;

    public static class ProposeStreetNameForMunicipalityMergerExtensions
    {
        public static ProposeStreetNamesForMunicipalityMerger WithMunicipalityId(
            this ProposeStreetNamesForMunicipalityMerger command, MunicipalityId municipalityId)
        {
            return new ProposeStreetNamesForMunicipalityMerger(
                municipalityId,
                command.StreetNames,
                command.Provenance);
        }

        public static ProposeStreetNamesForMunicipalityMerger WithStreetNames(
            this ProposeStreetNamesForMunicipalityMerger command, List<StreetNameToPropose> streetNames)
        {
            return new ProposeStreetNamesForMunicipalityMerger(
                command.MunicipalityId,
                streetNames,
                command.Provenance);
        }

        public static StreetNameToPropose WithRandomStreetName(
            this StreetNameToPropose streetName, Fixture fixture)
        {
            return new StreetNameToPropose(
                streetName.DesiredStatus,
                new Names(new List<StreetNameName> { fixture.Create<StreetNameName>() }),
                streetName.HomonymAdditions,
                streetName.PersistentLocalId,
                streetName.MergedStreetNamePersistentLocalIds);
        }

        public static StreetNameToPropose WithStreetNameNames(
            this StreetNameToPropose streetName, Names names)
        {
            return new StreetNameToPropose(
                streetName.DesiredStatus,
                names,
                streetName.HomonymAdditions,
                streetName.PersistentLocalId,
                streetName.MergedStreetNamePersistentLocalIds);
        }

        public static StreetNameToPropose WithHomonymAdditions(
            this StreetNameToPropose streetName, HomonymAdditions homonymAdditions)
        {
            return new StreetNameToPropose(
                streetName.DesiredStatus,
                streetName.StreetNameNames,
                homonymAdditions,
                streetName.PersistentLocalId,
                streetName.MergedStreetNamePersistentLocalIds);
        }

        public static StreetNameToPropose WithPersistentLocalId(
            this StreetNameToPropose streetName, PersistentLocalId persistentLocalId)
        {
            return new StreetNameToPropose(
                streetName.DesiredStatus,
                streetName.StreetNameNames,
                streetName.HomonymAdditions,
                persistentLocalId,
                streetName.MergedStreetNamePersistentLocalIds);
        }

        public static StreetNameToPropose WithMergedStreetNamePersistentIds(
            this StreetNameToPropose streetName, List<PersistentLocalId> mergedStreetNamePersistentIds)
        {
            return new StreetNameToPropose(
                streetName.DesiredStatus,
                streetName.StreetNameNames,
                streetName.HomonymAdditions,
                streetName.PersistentLocalId,
                mergedStreetNamePersistentIds);
        }

        public static StreetNameToPropose WithDesiredStatus(
            this StreetNameToPropose streetName, StreetNameStatus desiredStatus)
        {
            return new StreetNameToPropose(
                desiredStatus,
                streetName.StreetNameNames,
                streetName.HomonymAdditions,
                streetName.PersistentLocalId,
                streetName.MergedStreetNamePersistentLocalIds);
        }
    }
}
