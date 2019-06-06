namespace StreetNameRegistry.Infrastructure
{
    public class Schema
    {
        public const string Default = "StreetNameRegistry";
        
        public const string Import = "StreetNameRegistryImport";
        public const string Extract = "StreetNameRegistryExtract";
        public const string Legacy = "StreetNameRegistryLegacy";
        public const string Syndication = "StreetNameRegistrySyndication";
    }

    public class MigrationTables
    {
        public const string Legacy = "__EFMigrationsHistoryLegacy";
        public const string Extract = "__EFMigrationsHistoryExtract";
        public const string Syndication = "__EFMigrationsHistorySyndication";
    }
}
