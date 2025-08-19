using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreetNameRegistry.Projections.Syndication.Migrations
{
    /// <inheritdoc />
    public partial class AddMunicipalityIsRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MunicipalitySyndication",
                schema: "StreetNameRegistrySyndication");

            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityLatestSyndication_IsRemoved",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                column: "IsRemoved");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MunicipalityLatestSyndication_IsRemoved",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalityLatestSyndication");

            migrationBuilder.DropColumn(
                name: "IsRemoved",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalityLatestSyndication");

            migrationBuilder.CreateTable(
                name: "MunicipalitySyndication",
                schema: "StreetNameRegistrySyndication",
                columns: table => new
                {
                    MunicipalityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    NameDutch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEnglish = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameFrench = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameGerman = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NisCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Version = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MunicipalitySyndication", x => new { x.MunicipalityId, x.Position })
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalitySyndication_NisCode",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalitySyndication",
                column: "NisCode")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalitySyndication_Position",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalitySyndication",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalitySyndication_Version",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalitySyndication",
                column: "Version");
        }
    }
}
