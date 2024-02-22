using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreetNameRegistry.Projections.Integration.Migrations
{
    public partial class AddEventTypeToVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "type",
                schema: "integration_streetname",
                table: "streetname_versions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_streetname_versions_type",
                schema: "integration_streetname",
                table: "streetname_versions",
                column: "type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_streetname_versions_type",
                schema: "integration_streetname",
                table: "streetname_versions");

            migrationBuilder.DropColumn(
                name: "type",
                schema: "integration_streetname",
                table: "streetname_versions");
        }
    }
}
