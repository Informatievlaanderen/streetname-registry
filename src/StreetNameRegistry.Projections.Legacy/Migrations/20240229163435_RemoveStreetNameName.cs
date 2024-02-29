using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreetNameRegistry.Projections.Legacy.Migrations
{
    public partial class RemoveStreetNameName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StreetNameName",
                schema: "StreetNameRegistryLegacy");

            migrationBuilder.DropTable(
                name: "StreetNameNameV2",
                schema: "StreetNameRegistryLegacy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StreetNameName",
                schema: "StreetNameRegistryLegacy",
                columns: table => new
                {
                    StreetNameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Complete = table.Column<bool>(type: "bit", nullable: false),
                    IsFlemishRegion = table.Column<bool>(type: "bit", nullable: false),
                    NameDutch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameDutchSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameEnglish = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameEnglishSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameFrench = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameFrenchSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameGerman = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameGermanSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NisCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreetNameName", x => x.StreetNameId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "StreetNameNameV2",
                schema: "StreetNameRegistryLegacy",
                columns: table => new
                {
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    IsFlemishRegion = table.Column<bool>(type: "bit", nullable: false),
                    MunicipalityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameDutch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameDutchSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameEnglish = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameEnglishSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameFrench = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameFrenchSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameGerman = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameGermanSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NisCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreetNameNameV2", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameName_NameDutch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName",
                column: "NameDutch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameName_NameDutchSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName",
                column: "NameDutchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameName_NameEnglish",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName",
                column: "NameEnglish");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameName_NameEnglishSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName",
                column: "NameEnglishSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameName_NameFrench",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName",
                column: "NameFrench");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameName_NameFrenchSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName",
                column: "NameFrenchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameName_NameGerman",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName",
                column: "NameGerman");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameName_NameGermanSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName",
                column: "NameGermanSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameName_NisCode",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName",
                column: "NisCode");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameName_PersistentLocalId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName",
                column: "PersistentLocalId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameName_Removed_IsFlemishRegion_Complete",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName",
                columns: new[] { "Removed", "IsFlemishRegion", "Complete" });

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameName_Status",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameName_VersionTimestamp",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName",
                column: "VersionTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameNameV2_MunicipalityId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameNameV2",
                column: "MunicipalityId");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameNameV2_NameDutch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameNameV2",
                column: "NameDutch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameNameV2_NameDutchSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameNameV2",
                column: "NameDutchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameNameV2_NameEnglish",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameNameV2",
                column: "NameEnglish");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameNameV2_NameEnglishSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameNameV2",
                column: "NameEnglishSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameNameV2_NameFrench",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameNameV2",
                column: "NameFrench");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameNameV2_NameFrenchSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameNameV2",
                column: "NameFrenchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameNameV2_NameGerman",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameNameV2",
                column: "NameGerman");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameNameV2_NameGermanSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameNameV2",
                column: "NameGermanSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameNameV2_NisCode",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameNameV2",
                column: "NisCode");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameNameV2_Removed_IsFlemishRegion",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameNameV2",
                columns: new[] { "Removed", "IsFlemishRegion" });

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameNameV2_Status",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameNameV2",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameNameV2_VersionTimestamp",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameNameV2",
                column: "VersionTimestamp");
        }
    }
}
