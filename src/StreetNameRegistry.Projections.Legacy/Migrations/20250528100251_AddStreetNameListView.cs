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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
