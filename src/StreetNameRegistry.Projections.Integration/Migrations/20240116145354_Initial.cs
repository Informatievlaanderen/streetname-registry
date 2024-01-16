using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StreetNameRegistry.Projections.Integration.Migrations
{
    public partial class Initial : Migration
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
                    municipality_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    oslo_status = table.Column<string>(type: "text", nullable: false),
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
                    puri = table.Column<string>(type: "text", nullable: false),
                    @namespace = table.Column<string>(name: "namespace", type: "text", nullable: false),
                    version_as_string = table.Column<string>(type: "text", nullable: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
                    position = table.Column<long>(type: "bigint", nullable: false),
                    persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    municipality_id = table.Column<Guid>(type: "uuid", nullable: false),
                    streetname_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: true),
                    oslo_status = table.Column<string>(type: "text", nullable: true),
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
                    puri = table.Column<string>(type: "text", nullable: false),
                    @namespace = table.Column<string>(name: "namespace", type: "text", nullable: false),
                    version_as_string = table.Column<string>(type: "text", nullable: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedOnAsString = table.Column<string>(type: "text", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_streetname_versions", x => new { x.position, x.persistent_local_id });
                });

            migrationBuilder.CreateIndex(
                name: "IX_streetname_latest_items_is_removed",
                schema: "integration_streetname",
                table: "streetname_latest_items",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_streetname_latest_items_municipality_id",
                schema: "integration_streetname",
                table: "streetname_latest_items",
                column: "municipality_id");

            migrationBuilder.CreateIndex(
                name: "IX_streetname_latest_items_name_dutch",
                schema: "integration_streetname",
                table: "streetname_latest_items",
                column: "name_dutch");

            migrationBuilder.CreateIndex(
                name: "IX_streetname_latest_items_name_english",
                schema: "integration_streetname",
                table: "streetname_latest_items",
                column: "name_english");

            migrationBuilder.CreateIndex(
                name: "IX_streetname_latest_items_name_french",
                schema: "integration_streetname",
                table: "streetname_latest_items",
                column: "name_french");

            migrationBuilder.CreateIndex(
                name: "IX_streetname_latest_items_name_german",
                schema: "integration_streetname",
                table: "streetname_latest_items",
                column: "name_german");

            migrationBuilder.CreateIndex(
                name: "IX_streetname_latest_items_nis_code",
                schema: "integration_streetname",
                table: "streetname_latest_items",
                column: "nis_code");

            migrationBuilder.CreateIndex(
                name: "IX_streetname_latest_items_oslo_status",
                schema: "integration_streetname",
                table: "streetname_latest_items",
                column: "oslo_status");

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
                name: "IX_streetname_versions_municipality_id",
                schema: "integration_streetname",
                table: "streetname_versions",
                column: "municipality_id");

            migrationBuilder.CreateIndex(
                name: "IX_streetname_versions_nis_code",
                schema: "integration_streetname",
                table: "streetname_versions",
                column: "nis_code");

            migrationBuilder.CreateIndex(
                name: "IX_streetname_versions_oslo_status",
                schema: "integration_streetname",
                table: "streetname_versions",
                column: "oslo_status");

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

            migrationBuilder.CreateIndex(
                name: "IX_streetname_versions_version_timestamp",
                schema: "integration_streetname",
                table: "streetname_versions",
                column: "version_timestamp");
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
