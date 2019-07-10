using Microsoft.EntityFrameworkCore.Migrations;

namespace StreetNameRegistry.Projections.Legacy.Migrations
{
    public partial class RenameOsloId_PersistentLocalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OsloId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameVersions",
                newName: "PersistentLocalId");

            migrationBuilder.RenameIndex(
                name: "IX_StreetNameVersions_OsloId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameVersions",
                newName: "IX_StreetNameVersions_PersistentLocalId");

            migrationBuilder.RenameColumn(
                name: "OsloId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameSyndication",
                newName: "PersistentLocalId");

            migrationBuilder.RenameColumn(
                name: "OsloId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName",
                newName: "PersistentLocalId");

            migrationBuilder.RenameIndex(
                name: "IX_StreetNameName_OsloId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName",
                newName: "IX_StreetNameName_PersistentLocalId");

            migrationBuilder.RenameColumn(
                name: "OsloId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList",
                newName: "PersistentLocalId");

            migrationBuilder.RenameIndex(
                name: "IX_StreetNameList_OsloId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList",
                newName: "IX_StreetNameList_PersistentLocalId");

            migrationBuilder.RenameColumn(
                name: "OsloId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameDetails",
                newName: "PersistentLocalId");

            migrationBuilder.RenameIndex(
                name: "IX_StreetNameDetails_OsloId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameDetails",
                newName: "IX_StreetNameDetails_PersistentLocalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameVersions",
                newName: "OsloId");

            migrationBuilder.RenameIndex(
                name: "IX_StreetNameVersions_PersistentLocalId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameVersions",
                newName: "IX_StreetNameVersions_OsloId");

            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameSyndication",
                newName: "OsloId");

            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName",
                newName: "OsloId");

            migrationBuilder.RenameIndex(
                name: "IX_StreetNameName_PersistentLocalId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName",
                newName: "IX_StreetNameName_OsloId");

            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList",
                newName: "OsloId");

            migrationBuilder.RenameIndex(
                name: "IX_StreetNameList_PersistentLocalId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList",
                newName: "IX_StreetNameList_OsloId");

            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameDetails",
                newName: "OsloId");

            migrationBuilder.RenameIndex(
                name: "IX_StreetNameDetails_PersistentLocalId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameDetails",
                newName: "IX_StreetNameDetails_OsloId");
        }
    }
}
