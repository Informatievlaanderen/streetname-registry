using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StreetNameRegistry.Projections.Integration.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "integration_streetname");

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "integration_streetname",
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
                name: "streetname_latest_items",
                schema: "integration_streetname",
                columns: table => new
                {
                    persistent_local_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    status = table.Column<string>(type: "text", nullable: true),
                    nis_code = table.Column<string>(type: "text", nullable: true),
                    name_dutch = table.Column<string>(type: "text", nullable: true),
                    name_french = table.Column<string>(type: "text", nullable: true),
                    name_german = table.Column<string>(type: "text", nullable: true),
                    name_english = table.Column<string>(type: "text", nullable: true),
                    homonym_addition_dutch = table.Column<string>(type: "text", nullable: true),
                    homonym_addition_french = table.Column<string>(type: "text", nullable: true),
                    homonym_addition_german = table.Column<string>(type: "text", nullable: true),
                    homonym_addition_english = table.Column<string>(type: "text", nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false),
                    puri_id = table.Column<string>(type: "text", nullable: false),
                    @namespace = table.Column<string>(name: "namespace", type: "text", nullable: false),
                    version_as_string = table.Column<string>(type: "text", nullable: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    idempotence_key = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_streetname_latest_items", x => x.persistent_local_id);
                });

            migrationBuilder.CreateTable(
                name: "streetname_versions",
                schema: "integration_streetname",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    streetname_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: true),
                    nis_code = table.Column<string>(type: "text", nullable: true),
                    name_dutch = table.Column<string>(type: "text", nullable: true),
                    name_french = table.Column<string>(type: "text", nullable: true),
                    name_german = table.Column<string>(type: "text", nullable: true),
                    name_english = table.Column<string>(type: "text", nullable: true),
                    homonym_addition_dutch = table.Column<string>(type: "text", nullable: true),
                    homonym_addition_french = table.Column<string>(type: "text", nullable: true),
                    homonym_addition_german = table.Column<string>(type: "text", nullable: true),
                    homonym_addition_english = table.Column<string>(type: "text", nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false),
                    puri_id = table.Column<string>(type: "text", nullable: false),
                    @namespace = table.Column<string>(name: "namespace", type: "text", nullable: false),
                    version_as_string = table.Column<string>(type: "text", nullable: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_streetname_versions", x => x.position);
                });

            migrationBuilder.CreateIndex(
                name: "IX_streetname_latest_items_is_removed",
                schema: "integration_streetname",
                table: "streetname_latest_items",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_streetname_latest_items_name_dutch",
                schema: "integration_streetname",
                table: "streetname_latest_items",
                column: "name_dutch");

            migrationBuilder.CreateIndex(
                name: "IX_streetname_latest_items_persistent_local_id",
                schema: "integration_streetname",
                table: "streetname_latest_items",
                column: "persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_streetname_latest_items_status",
                schema: "integration_streetname",
                table: "streetname_latest_items",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_streetname_versions_is_removed",
                schema: "integration_streetname",
                table: "streetname_versions",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_streetname_versions_persistent_local_id",
                schema: "integration_streetname",
                table: "streetname_versions",
                column: "persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_streetname_versions_status",
                schema: "integration_streetname",
                table: "streetname_versions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_streetname_versions_streetname_id",
                schema: "integration_streetname",
                table: "streetname_versions",
                column: "streetname_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "integration_streetname");

            migrationBuilder.DropTable(
                name: "streetname_latest_items",
                schema: "integration_streetname");

            migrationBuilder.DropTable(
                name: "streetname_versions",
                schema: "integration_streetname");
        }
    }
}
