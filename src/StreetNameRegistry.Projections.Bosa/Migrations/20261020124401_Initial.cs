using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreetNameRegistry.Projections.Bosa.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "integration_bosa_streetname");

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "integration_bosa_streetname",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    DesiredState = table.Column<string>(type: "text", nullable: true),
                    DesiredStateChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "streetname_changes",
                schema: "integration_bosa_streetname",
                columns: table => new
                {
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: false),
                    position = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_streetname_changes", x => new { x.day, x.persistent_local_id });
                });

            migrationBuilder.CreateTable(
                name: "streetnames",
                schema: "integration_bosa_streetname",
                columns: table => new
                {
                    persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    municipality_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nis_code = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    name_dutch = table.Column<string>(type: "text", nullable: true),
                    name_french = table.Column<string>(type: "text", nullable: true),
                    name_german = table.Column<string>(type: "text", nullable: true),
                    name_english = table.Column<string>(type: "text", nullable: true),
                    homonym_addition_dutch = table.Column<string>(type: "text", nullable: true),
                    homonym_addition_french = table.Column<string>(type: "text", nullable: true),
                    homonym_addition_german = table.Column<string>(type: "text", nullable: true),
                    homonym_addition_english = table.Column<string>(type: "text", nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false),
                    @namespace = table.Column<string>(name: "namespace", type: "text", nullable: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version_id = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    begin_life_span = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    valid_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_streetnames", x => x.persistent_local_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_streetnames_municipality_id",
                schema: "integration_bosa_streetname",
                table: "streetnames",
                column: "municipality_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "integration_bosa_streetname");

            migrationBuilder.DropTable(
                name: "streetname_changes",
                schema: "integration_bosa_streetname");

            migrationBuilder.DropTable(
                name: "streetnames",
                schema: "integration_bosa_streetname");
        }
    }
}
