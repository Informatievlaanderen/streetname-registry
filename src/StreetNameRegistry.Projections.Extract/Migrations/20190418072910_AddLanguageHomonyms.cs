using Microsoft.EntityFrameworkCore.Migrations;

namespace StreetNameRegistry.Projections.Extract.Migrations
{
    public partial class AddLanguageHomonyms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HomonymDutch",
                schema: "StreetNameRegistryExtract",
                table: "StreetName",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomonymEnglish",
                schema: "StreetNameRegistryExtract",
                table: "StreetName",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomonymFrench",
                schema: "StreetNameRegistryExtract",
                table: "StreetName",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomonymGerman",
                schema: "StreetNameRegistryExtract",
                table: "StreetName",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomonymUnknown",
                schema: "StreetNameRegistryExtract",
                table: "StreetName",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HomonymDutch",
                schema: "StreetNameRegistryExtract",
                table: "StreetName");

            migrationBuilder.DropColumn(
                name: "HomonymEnglish",
                schema: "StreetNameRegistryExtract",
                table: "StreetName");

            migrationBuilder.DropColumn(
                name: "HomonymFrench",
                schema: "StreetNameRegistryExtract",
                table: "StreetName");

            migrationBuilder.DropColumn(
                name: "HomonymGerman",
                schema: "StreetNameRegistryExtract",
                table: "StreetName");

            migrationBuilder.DropColumn(
                name: "HomonymUnknown",
                schema: "StreetNameRegistryExtract",
                table: "StreetName");
        }
    }
}
