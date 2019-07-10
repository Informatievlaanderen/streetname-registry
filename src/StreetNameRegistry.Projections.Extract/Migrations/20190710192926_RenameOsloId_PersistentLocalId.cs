using Microsoft.EntityFrameworkCore.Migrations;

namespace StreetNameRegistry.Projections.Extract.Migrations
{
    public partial class RenameOsloId_PersistentLocalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StreetNameOsloId",
                schema: "StreetNameRegistryExtract",
                table: "StreetName",
                newName: "StreetNamePersistentLocalId");

            migrationBuilder.RenameIndex(
                name: "IX_StreetName_StreetNameOsloId",
                schema: "StreetNameRegistryExtract",
                table: "StreetName",
                newName: "IX_StreetName_StreetNamePersistentLocalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StreetNamePersistentLocalId",
                schema: "StreetNameRegistryExtract",
                table: "StreetName",
                newName: "StreetNameOsloId");

            migrationBuilder.RenameIndex(
                name: "IX_StreetName_StreetNamePersistentLocalId",
                schema: "StreetNameRegistryExtract",
                table: "StreetName",
                newName: "IX_StreetName_StreetNameOsloId");
        }
    }
}
