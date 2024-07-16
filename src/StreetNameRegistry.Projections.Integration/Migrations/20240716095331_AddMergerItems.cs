using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreetNameRegistry.Projections.Integration.Migrations
{
    /// <inheritdoc />
    public partial class AddMergerItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "streetname_merger_items",
                schema: "integration_streetname",
                columns: table => new
                {
                    new_persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    merged_persistent_local_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_streetname_merger_items", x => new { x.new_persistent_local_id, x.merged_persistent_local_id });
                });

            migrationBuilder.CreateIndex(
                name: "IX_streetname_merger_items_merged_persistent_local_id",
                schema: "integration_streetname",
                table: "streetname_merger_items",
                column: "merged_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_streetname_merger_items_new_persistent_local_id",
                schema: "integration_streetname",
                table: "streetname_merger_items",
                column: "new_persistent_local_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "streetname_merger_items",
                schema: "integration_streetname");
        }
    }
}
