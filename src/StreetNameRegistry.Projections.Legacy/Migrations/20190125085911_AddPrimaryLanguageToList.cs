using Microsoft.EntityFrameworkCore.Migrations;

namespace StreetNameRegistry.Projections.Legacy.Migrations
{
    public partial class AddPrimaryLanguageToList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrimaryLanguage",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryLanguage",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList");
        }
    }
}
