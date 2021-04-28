using Microsoft.EntityFrameworkCore.Migrations;

namespace StreetNameRegistry.Projections.Legacy.Migrations
{
    public partial class AddStatusIndexOnList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StreetNameList_Status",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList",
                column: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StreetNameList_Status",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList");
        }
    }
}
