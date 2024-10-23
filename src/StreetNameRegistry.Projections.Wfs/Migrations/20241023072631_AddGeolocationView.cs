using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreetNameRegistry.Projections.Wfs.Migrations
{
    /// <inheritdoc />
    public partial class AddGeolocationView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "geolocation");
            migrationBuilder.Sql("ALTER AUTHORIZATION ON SCHEMA::geolocation TO wms");
            migrationBuilder.Sql(@"
CREATE VIEW geolocation.StreetNameOsloGeolocationView WITH SCHEMABINDING AS
SELECT
	   CASE WHEN Removed = 1 THEN NULL ELSE CONCAT('https://data.vlaanderen.be/id/straatnaam/', [PersistentLocalId]) END as IDENTIFICATOR_ID
      ,CASE WHEN Removed = 1 THEN NULL ELSE 'https://data.vlaanderen.be/id/straatnaam' END as IDENTIFICATOR_NAAMRUIMTE
      ,CASE WHEN Removed = 1 THEN NULL ELSE [PersistentLocalId] END as IDENTIFICATOR_OBJECTID
	  ,CASE WHEN Removed = 1 THEN NULL ELSE [VersionAsString] END as IDENTIFICATOR_VERSIEID
	  ,CASE WHEN Removed = 1 THEN NULL ELSE [NisCode] END as GEMEENTE_OBJECTID
      ,CASE WHEN Removed = 1 THEN NULL ELSE [NameDutch] END as STRAATNAAM_NL
      ,CASE WHEN Removed = 1 THEN NULL ELSE [NameFrench] END as STRAATNAAM_FR
      ,CASE WHEN Removed = 1 THEN NULL ELSE [NameGerman] END as STRAATNAAM_DE
      ,CASE WHEN Removed = 1 THEN NULL ELSE [HomonymAdditionDutch] END as HOMONIEMTOEVOEGING_NL
      ,CASE WHEN Removed = 1 THEN NULL ELSE [HomonymAdditionFrench] END as HOMONIEMTOEVOEGING_FR
      ,CASE WHEN Removed = 1 THEN NULL ELSE [HomonymAdditionGerman] END as HOMONIEMTOEVOEGING_DE
      ,CASE WHEN Removed = 1 THEN NULL ELSE
			CASE
				WHEN [Status] = 0 THEN 'voorgesteld'
				WHEN [Status] = 1 THEN 'inGebruik'
				WHEN [Status] = 2 THEN 'gehistoreerd'
				WHEN [Status] = 3 THEN 'afgekeurd'
			END
		END as STRAATNAAMSTATUS
      ,[Removed] as REMOVED
      ,[PersistentLocalId] as [msgkey]
  FROM [wfs.streetname].[StreetNameHelperV2]");

            migrationBuilder.Sql("CREATE UNIQUE CLUSTERED INDEX IX_StreetNameGeolocationView_ObjectId ON [geolocation].[StreetNameOsloGeolocationView] ([msgkey])");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW geolocation.StreetNameOsloGeolocationView;");
        }
    }
}
