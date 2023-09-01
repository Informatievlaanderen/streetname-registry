using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreetNameRegistry.Projections.Wms.Migrations
{
    public partial class RecreateWmsIndexRemoved : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StreetNameHelperV2_Removed",
                schema: "wms.streetname",
                table: "StreetNameHelperV2");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameHelperV2_Removed",
                schema: "wms.streetname",
                table: "StreetNameHelperV2",
                column: "Removed")
                .Annotation("SqlServer:Include", new[] { "NisCode" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StreetNameHelperV2_Removed",
                schema: "wms.streetname",
                table: "StreetNameHelperV2");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameHelperV2_Removed",
                schema: "wms.streetname",
                table: "StreetNameHelperV2",
                column: "Removed");
        }
    }
}
