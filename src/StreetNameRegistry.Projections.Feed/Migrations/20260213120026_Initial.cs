using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreetNameRegistry.Projections.Feed.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "StreetNameRegistryFeed");

            migrationBuilder.CreateSequence(
                name: "StreetNameFeedSequence",
                schema: "StreetNameRegistryFeed");

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "StreetNameRegistryFeed",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    DesiredState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DesiredStateChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "StreetNameDocuments",
                schema: "StreetNameRegistryFeed",
                columns: table => new
                {
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    Document = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastChangedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RecordCreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreetNameDocuments", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "StreetNameFeed",
                schema: "StreetNameRegistryFeed",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Page = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Application = table.Column<int>(type: "int", nullable: true),
                    Modification = table.Column<int>(type: "int", nullable: true),
                    Operator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Organisation = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CloudEvent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreetNameFeed", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameFeed_Page",
                schema: "StreetNameRegistryFeed",
                table: "StreetNameFeed",
                column: "Page");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameFeed_PersistentLocalId",
                schema: "StreetNameRegistryFeed",
                table: "StreetNameFeed",
                column: "PersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameFeed_Position",
                schema: "StreetNameRegistryFeed",
                table: "StreetNameFeed",
                column: "Position");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "StreetNameRegistryFeed");

            migrationBuilder.DropTable(
                name: "StreetNameDocuments",
                schema: "StreetNameRegistryFeed");

            migrationBuilder.DropTable(
                name: "StreetNameFeed",
                schema: "StreetNameRegistryFeed");

            migrationBuilder.DropSequence(
                name: "StreetNameFeedSequence",
                schema: "StreetNameRegistryFeed");
        }
    }
}
