using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreetNameRegistry.Producer.Ldes.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "StreetNameRegistryProducerLdes");

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "StreetNameRegistryProducerLdes",
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
                name: "StreetName",
                schema: "StreetNameRegistryProducerLdes",
                columns: table => new
                {
                    StreetNamePersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    MunicipalityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NisCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NameDutch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameFrench = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEnglish = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameGerman = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomonymAdditionDutch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomonymAdditionFrench = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomonymAdditionEnglish = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomonymAdditionGerman = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreetName", x => x.StreetNamePersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StreetName_NisCode",
                schema: "StreetNameRegistryProducerLdes",
                table: "StreetName",
                column: "NisCode")
                .Annotation("SqlServer:Clustered", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "StreetNameRegistryProducerLdes");

            migrationBuilder.DropTable(
                name: "StreetName",
                schema: "StreetNameRegistryProducerLdes");
        }
    }
}
