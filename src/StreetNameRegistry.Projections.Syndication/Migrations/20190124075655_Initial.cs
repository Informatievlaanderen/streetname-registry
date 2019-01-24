using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StreetNameRegistry.Projections.Syndication.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "StreetNameRegistrySyndication");

            migrationBuilder.CreateTable(
                name: "MunicipalityLatestSyndication",
                schema: "StreetNameRegistrySyndication",
                columns: table => new
                {
                    MunicipalityId = table.Column<Guid>(nullable: false),
                    NisCode = table.Column<string>(nullable: true),
                    NameDutch = table.Column<string>(nullable: true),
                    NameDutchSearch = table.Column<string>(nullable: true),
                    NameFrench = table.Column<string>(nullable: true),
                    NameFrenchSearch = table.Column<string>(nullable: true),
                    NameGerman = table.Column<string>(nullable: true),
                    NameGermanSearch = table.Column<string>(nullable: true),
                    NameEnglish = table.Column<string>(nullable: true),
                    NameEnglishSearch = table.Column<string>(nullable: true),
                    PrimaryLanguage = table.Column<int>(nullable: true),
                    Version = table.Column<DateTimeOffset>(nullable: true),
                    Position = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MunicipalityLatestSyndication", x => x.MunicipalityId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "MunicipalitySyndication",
                schema: "StreetNameRegistrySyndication",
                columns: table => new
                {
                    MunicipalityId = table.Column<Guid>(nullable: false),
                    Position = table.Column<long>(nullable: false),
                    NisCode = table.Column<string>(nullable: true),
                    NameDutch = table.Column<string>(nullable: true),
                    NameFrench = table.Column<string>(nullable: true),
                    NameGerman = table.Column<string>(nullable: true),
                    NameEnglish = table.Column<string>(nullable: true),
                    Version = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MunicipalitySyndication", x => new { x.MunicipalityId, x.Position })
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "StreetNameRegistrySyndication",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Position = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityLatestSyndication_NameDutchSearch",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                column: "NameDutchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityLatestSyndication_NameEnglishSearch",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                column: "NameEnglishSearch");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityLatestSyndication_NameFrenchSearch",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                column: "NameFrenchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityLatestSyndication_NameGermanSearch",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                column: "NameGermanSearch");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityLatestSyndication_NisCode",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                column: "NisCode")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityLatestSyndication_Position",
                schema: "StreetNameRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                column: "Position");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MunicipalityLatestSyndication",
                schema: "StreetNameRegistrySyndication");

            migrationBuilder.DropTable(
                name: "MunicipalitySyndication",
                schema: "StreetNameRegistrySyndication");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "StreetNameRegistrySyndication");
        }
    }
}
