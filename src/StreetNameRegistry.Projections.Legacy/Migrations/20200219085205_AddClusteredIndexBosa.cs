using Microsoft.EntityFrameworkCore.Migrations;

namespace StreetNameRegistry.Projections.Legacy.Migrations
{
    public partial class AddClusteredIndexBosa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StreetNameName_PersistentLocalId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameName_PersistentLocalId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName",
                column: "PersistentLocalId")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StreetNameName_PersistentLocalId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameName_PersistentLocalId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameName",
                column: "PersistentLocalId");
        }
    }
}
