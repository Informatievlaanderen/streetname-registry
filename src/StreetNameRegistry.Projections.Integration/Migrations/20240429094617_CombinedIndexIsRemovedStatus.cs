﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreetNameRegistry.Projections.Integration.Migrations
{
    /// <inheritdoc />
    public partial class CombinedIndexIsRemovedStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_streetname_latest_items_is_removed_status",
                schema: "integration_streetname",
                table: "streetname_latest_items",
                columns: new[] { "is_removed", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_streetname_latest_items_is_removed_status",
                schema: "integration_streetname",
                table: "streetname_latest_items");
        }
    }
}
