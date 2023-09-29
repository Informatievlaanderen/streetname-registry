using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreetNameRegistry.Projections.Legacy.Migrations
{
    public partial class AddIsInFlemishRegion_List : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInFlemishRegion",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameListV2",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                "UPDATE StreetNameRegistryLegacy.StreetNameListV2 SET IsInFlemishRegion = 1 WHERE NisCode LIKE '1%' OR NisCode LIKE '3%' OR NisCode LIKE '4%' OR NisCode LIKE '7%' OR NisCode LIKE '23%' OR NisCode LIKE '24%'");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameListV2_IsInFlemishRegion",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameListV2",
                column: "IsInFlemishRegion");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StreetNameListV2_IsInFlemishRegion",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameListV2");

            migrationBuilder.DropColumn(
                name: "IsInFlemishRegion",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameListV2");
        }
    }
}
