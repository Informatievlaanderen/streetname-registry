using Microsoft.EntityFrameworkCore.Migrations;

namespace StreetNameRegistry.Projections.Legacy.Migrations
{
    public partial class AddStreetNameFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameDutchSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEnglishSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameFrenchSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameGermanSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameList_NameDutchSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList",
                column: "NameDutchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameList_NameEnglishSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList",
                column: "NameEnglishSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameList_NameFrenchSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList",
                column: "NameFrenchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameList_NameGermanSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList",
                column: "NameGermanSearch");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StreetNameList_NameDutchSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList");

            migrationBuilder.DropIndex(
                name: "IX_StreetNameList_NameEnglishSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList");

            migrationBuilder.DropIndex(
                name: "IX_StreetNameList_NameFrenchSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList");

            migrationBuilder.DropIndex(
                name: "IX_StreetNameList_NameGermanSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList");

            migrationBuilder.DropColumn(
                name: "NameDutchSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList");

            migrationBuilder.DropColumn(
                name: "NameEnglishSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList");

            migrationBuilder.DropColumn(
                name: "NameFrenchSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList");

            migrationBuilder.DropColumn(
                name: "NameGermanSearch",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList");
        }
    }
}
