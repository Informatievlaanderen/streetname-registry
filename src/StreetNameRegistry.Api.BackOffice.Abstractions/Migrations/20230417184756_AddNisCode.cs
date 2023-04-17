using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreetNameRegistry.Api.BackOffice.Abstractions.Migrations
{
    using Infrastructure;

    public partial class AddNisCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NisCode",
                schema: "StreetNameRegistryBackOffice",
                table: "MunicipalityIdByPersistentLocalId",
                type: "nvarchar(450)",
                nullable: true,
                defaultValue: null);

            migrationBuilder.Sql($"UPDATE [{Schema.BackOffice}].[MunicipalityIdByPersistentLocalId] SET [NisCode] = (SELECT [NisCode] FROM [streetname-registry].[{Schema.Consumer}].[MunicipalityConsumerItems] WHERE [MunicipalityId] = [MunicipalityIdByPersistentLocalId].[MunicipalityId])");

            migrationBuilder.AlterColumn<string>(
                name: "NisCode",
                schema: "StreetNameRegistryBackOffice",
                table: "MunicipalityIdByPersistentLocalId",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityIdByPersistentLocalId_NisCode",
                schema: "StreetNameRegistryBackOffice",
                table: "MunicipalityIdByPersistentLocalId",
                column: "NisCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MunicipalityIdByPersistentLocalId_NisCode",
                schema: "StreetNameRegistryBackOffice",
                table: "MunicipalityIdByPersistentLocalId");

            migrationBuilder.DropColumn(
                name: "NisCode",
                schema: "StreetNameRegistryBackOffice",
                table: "MunicipalityIdByPersistentLocalId");
        }
    }
}
