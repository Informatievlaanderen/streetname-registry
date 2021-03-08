using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StreetNameRegistry.Projections.Legacy.Migrations
{
    public partial class RemoveAddressVersions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StreetNameVersions",
                schema: "StreetNameRegistryLegacy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StreetNameVersions",
                schema: "StreetNameRegistryLegacy",
                columns: table => new
                {
                    StreetNameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    Application = table.Column<int>(type: "int", nullable: true),
                    Complete = table.Column<bool>(type: "bit", nullable: false),
                    HomonymAdditionDutch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomonymAdditionEnglish = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomonymAdditionFrench = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomonymAdditionGerman = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modification = table.Column<int>(type: "int", nullable: true),
                    NameDutch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEnglish = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameFrench = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameGerman = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NisCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Operator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Organisation = table.Column<int>(type: "int", nullable: true),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreetNameVersions", x => new { x.StreetNameId, x.Position })
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameVersions_PersistentLocalId",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameVersions",
                column: "PersistentLocalId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameVersions_Removed",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameVersions",
                column: "Removed");
        }
    }
}
