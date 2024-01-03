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
                name: "integration");

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "integration",
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
                name: "StreetNameLatestItems",
                schema: "integration",
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
                    table.PrimaryKey("PK_StreetNameLatestItems", x => x.persistent_local_id);
                });

            migrationBuilder.CreateTable(
                name: "StreetNameVersions",
                schema: "integration",
                columns: table => new
                {
                    Position = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    persistent_local_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_StreetNameVersions", x => x.Position);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestItems_is_removed",
                schema: "integration",
                table: "StreetNameLatestItems",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestItems_name_dutch",
                schema: "integration",
                table: "StreetNameLatestItems",
                column: "name_dutch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestItems_persistent_local_id",
                schema: "integration",
                table: "StreetNameLatestItems",
                column: "persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestItems_status",
                schema: "integration",
                table: "StreetNameLatestItems",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameVersions_is_removed",
                schema: "integration",
                table: "StreetNameVersions",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameVersions_name_dutch",
                schema: "integration",
                table: "StreetNameVersions",
                column: "name_dutch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameVersions_persistent_local_id",
                schema: "integration",
                table: "StreetNameVersions",
                column: "persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameVersions_Position",
                schema: "integration",
                table: "StreetNameVersions",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameVersions_status",
                schema: "integration",
                table: "StreetNameVersions",
                column: "status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "integration");

            migrationBuilder.DropTable(
                name: "StreetNameLatestItems",
                schema: "integration");

            migrationBuilder.DropTable(
                name: "StreetNameVersions",
                schema: "integration");
        }
    }
}
