using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StreetNameRegistry.Projections.Legacy.Migrations
{
    public partial class AddSyndicationItemCreatedAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // set value to UtcNow for all existing records
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SyndicationItemCreatedAt",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameSyndication",
                nullable: false,
                defaultValue: DateTimeOffset.UtcNow);

            // remove the default value
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "SyndicationItemCreatedAt",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameSyndication",
                nullable: false,
                defaultValue: null);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SyndicationItemCreatedAt",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameSyndication");
        }
    }
}
