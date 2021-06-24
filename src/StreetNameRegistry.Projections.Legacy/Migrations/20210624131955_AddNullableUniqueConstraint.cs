using Microsoft.EntityFrameworkCore.Migrations;

namespace StreetNameRegistry.Projections.Legacy.Migrations
{
    public partial class AddNullableUniqueConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StreetNameList_PersistentLocalId_1",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList",
                column: "PersistentLocalId",
                unique: true,
                filter: "([PersistentLocalId] IS NOT NULL)")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameDetails_PersistentLocalId_1",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameDetails",
                column: "PersistentLocalId",
                unique: true,
                filter: "([PersistentLocalId] IS NOT NULL)")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StreetNameList_PersistentLocalId_1",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList");

            migrationBuilder.DropIndex(
                name: "IX_StreetNameDetails_PersistentLocalId_1",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameDetails");
        }
    }
}
