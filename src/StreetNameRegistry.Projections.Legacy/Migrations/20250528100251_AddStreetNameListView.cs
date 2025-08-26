using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreetNameRegistry.Projections.Legacy.Migrations
{
    /// <inheritdoc />
    public partial class AddStreetNameListView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 DROP VIEW IF EXISTS [StreetNameRegistryLegacy].vw_StreetNameList
                                 GO
                                 CREATE VIEW [StreetNameRegistryLegacy].vw_StreetNameList WITH SCHEMABINDING AS
                                 SELECT
                                 	s.PersistentLocalId as StreetNamePersistentLocalId
                                 	, s.NisCode as NisCode
                                 	, s.NameDutch as StreetNameDutch
                                 	, s.NameFrench as StreetNameFrench
                                 	, s.NameGerman as StreetNameGerman
                                 	, s.NameEnglish as StreetNameEnglish
                                 	, s.NameDutchSearch as StreetNameDutchSearch
                                 	, s.NameFrenchSearch as StreetNameFrenchSearch
                                 	, s.NameGermanSearch as StreetNameGermanSearch
                                 	, s.NameEnglishSearch as StreetNameEnglishSearch
                                 	, s.HomonymAdditionDutch as StreetNameHomonymAdditionDutch
                                 	, s.HomonymAdditionFrench as StreetNameHomonymAdditionFrench
                                 	, s.HomonymAdditionGerman as StreetNameHomonymAdditionGerman
                                 	, s.HomonymAdditionEnglish as StreetNameHomonymAdditionEnglish
                                 	, s.[Status] as StreetNameStatus
                                 	, m.NameDutchSearch as MunicipalityNameDutchSearch
                                 	, m.NameFrenchSearch as MunicipalityNameFrenchSearch
                                 	, m.NameGermanSearch as MunicipalityNameGermanSearch
                                 	, m.NameEnglishSearch as MunicipalityNameEnglishSearch
                                 	, s.PrimaryLanguage
                                 	, s.IsInFlemishRegion
                                 	, s.VersionTimestamp
                                 FROM [StreetNameRegistryLegacy].StreetNameListV2 s
                                 JOIN [StreetNameRegistrySyndication].MunicipalityLatestSyndication m ON s.MunicipalityId = m.MunicipalityId
                                 WHERE s.Removed = 0
                                 """);

            migrationBuilder.Sql("CREATE UNIQUE CLUSTERED INDEX IX_vw_StreetNameList_StreetNamePersistentLocalId ON [StreetNameRegistryLegacy].[vw_StreetNameList] ([StreetNamePersistentLocalId])");

            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX IX_vw_StreetNameList_MuniDutchSearch ON [StreetNameRegistryLegacy].[vw_StreetNameList] (MunicipalityNameDutchSearch, StreetNamePersistentLocalId)");

            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX IX_vw_StreetNameList_MuniFrenchSearch ON [StreetNameRegistryLegacy].[vw_StreetNameList] (MunicipalityNameFrenchSearch, StreetNamePersistentLocalId)");

            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX IX_vw_StreetNameList_MuniEnglishSearch ON [StreetNameRegistryLegacy].[vw_StreetNameList] (MunicipalityNameEnglishSearch, StreetNamePersistentLocalId)");

            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX IX_vw_StreetNameList_MuniGermanSearch ON [StreetNameRegistryLegacy].[vw_StreetNameList] (MunicipalityNameGermanSearch, StreetNamePersistentLocalId)");

            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX IX_vw_StreetNameList_StreetNameDutchSearch ON [StreetNameRegistryLegacy].[vw_StreetNameList] (StreetNameDutchSearch, StreetNamePersistentLocalId)");

            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX IX_vw_StreetNameList_StreetNameFrenchSearch ON [StreetNameRegistryLegacy].[vw_StreetNameList] (StreetNameFrenchSearch, StreetNamePersistentLocalId)");

            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX IX_vw_StreetNameList_StreetNameEnglishSearch ON [StreetNameRegistryLegacy].[vw_StreetNameList] (StreetNameEnglishSearch, StreetNamePersistentLocalId)");

            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX IX_vw_StreetNameList_StreetNameGermanSearch ON [StreetNameRegistryLegacy].[vw_StreetNameList] (StreetNameGermanSearch, StreetNamePersistentLocalId)");

            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX IX_vw_StreetNameList_Status ON [StreetNameRegistryLegacy].[vw_StreetNameList] (StreetNameStatus, StreetNamePersistentLocalId)");

            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX IX_vw_StreetNameList_NisCode ON [StreetNameRegistryLegacy].[vw_StreetNameList] (NisCode, StreetNamePersistentLocalId)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
