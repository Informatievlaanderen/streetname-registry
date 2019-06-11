using Microsoft.EntityFrameworkCore.Migrations;

namespace StreetNameRegistry.Projections.Legacy.Migrations
{
    public partial class RenamePlanToReason : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Plan",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameVersions");

            migrationBuilder.DropColumn(
                name: "Plan",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameSyndication");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameVersions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameSyndication",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameVersions");

            migrationBuilder.DropColumn(
                name: "Reason",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameSyndication");

            migrationBuilder.AddColumn<int>(
                name: "Plan",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameVersions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Plan",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameSyndication",
                nullable: true);
        }
    }
}
