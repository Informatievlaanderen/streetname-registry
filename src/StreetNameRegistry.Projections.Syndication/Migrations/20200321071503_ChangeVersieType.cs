using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StreetNameRegistry.Projections.Syndication.Migrations
{
    public partial class ChangeVersieType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MunicipalitySyndication_Version",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalitySyndication");

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalitySyndication",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalitySyndication_Version",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalitySyndication",
                column: "Version");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MunicipalitySyndication_Version",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalitySyndication");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Version",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalitySyndication",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Version",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalitySyndication_Version",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalitySyndication",
                column: "Version");
        }
    }
}
