namespace StreetNameRegistry.Infrastructure
{
    public static class Schema
    {
        public const string Default = "StreetNameRegistry";
        public const string Import = "StreetNameRegistryImport";
        public const string Extract = "StreetNameRegistryExtract";
        public const string Legacy = "StreetNameRegistryLegacy";
        public const string Syndication = "StreetNameRegistrySyndication";
        public const string Sequence = "StreetNameRegistrySequence";
        public const string Consumer = "StreetNameRegistryConsumer";
        public const string Wfs = "wfs.streetname";
        public const string Wms = "wms.streetname";
        public const string MigrateStreetName = "StreetNameRegistryMigration";
        public const string BackOffice = "StreetNameRegistryBackOffice";
        public const string BackOfficeProjections = "StreetNameRegistryBackOfficeProjections";
        public const string Producer = "StreetNameRegistryProducer";
        public const string ProducerSnapshotOslo = "StreetNameRegistryProducerSnapshotOslo";
    }

    public static class MigrationTables
    {
        public const string Legacy = "__EFMigrationsHistoryLegacy";
        public const string Extract = "__EFMigrationsHistoryExtract";
        public const string Syndication = "__EFMigrationsHistorySyndication";
        public const string RedisDataMigration = "__EFMigrationsHistoryDataMigration";
        public const string Sequence = "__EFMigrationsHistorySequence";
        public const string Consumer = "__EFMigrationsHistoryConsumer";
        public const string Wfs = "__EFMigrationsHistoryWfsStreetName";
        public const string Wms = "__EFMigrationsHistoryWmsStreetName";
        public const string BackOffice = "__EFMigrationsHistoryBackOffice";
        public const string BackOfficeProjections = "__EFMigrationsHistoryBackOfficeProjections";
        public const string Producer = "__EFMigrationsHistoryProducer";
        public const string ProducerSnapshotOslo = "__EFMigrationsHistoryProducerSnapshotOslo";
    }
}
