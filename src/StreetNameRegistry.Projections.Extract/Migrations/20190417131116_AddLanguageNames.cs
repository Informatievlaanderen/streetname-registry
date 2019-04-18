using Microsoft.EntityFrameworkCore.Migrations;

namespace StreetNameRegistry.Projections.Extract.Migrations
{
    public partial class AddLanguageNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChosenLanguage",
                schema: "StreetNameRegistryExtract",
                table: "StreetName");

            migrationBuilder.AddColumn<string>(
                name: "NameDutch",
                schema: "StreetNameRegistryExtract",
                table: "StreetName",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEnglish",
                schema: "StreetNameRegistryExtract",
                table: "StreetName",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameFrench",
                schema: "StreetNameRegistryExtract",
                table: "StreetName",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameGerman",
                schema: "StreetNameRegistryExtract",
                table: "StreetName",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameUnknown",
                schema: "StreetNameRegistryExtract",
                table: "StreetName",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameDutch",
                schema: "StreetNameRegistryExtract",
                table: "StreetName");

            migrationBuilder.DropColumn(
                name: "NameEnglish",
                schema: "StreetNameRegistryExtract",
                table: "StreetName");

            migrationBuilder.DropColumn(
                name: "NameFrench",
                schema: "StreetNameRegistryExtract",
                table: "StreetName");

            migrationBuilder.DropColumn(
                name: "NameGerman",
                schema: "StreetNameRegistryExtract",
                table: "StreetName");

            migrationBuilder.DropColumn(
                name: "NameUnknown",
                schema: "StreetNameRegistryExtract",
                table: "StreetName");

            migrationBuilder.AddColumn<int>(
                name: "ChosenLanguage",
                schema: "StreetNameRegistryExtract",
                table: "StreetName",
                nullable: true);
        }
    }
}
