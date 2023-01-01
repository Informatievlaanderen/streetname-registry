namespace StreetNameRegistry.Projections.Legacy.Migrations
{
    using global::Microsoft.EntityFrameworkCore.Migrations;
    using Infrastructure;
    using StreetNameDetail;
    using StreetNameList;

    public partial class AddStatusToList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList",
                nullable: true);

            migrationBuilder.Sql(@$"MERGE INTO [{Schema.Legacy}].[{StreetNameListConfiguration.TableName}] list
                                        USING [{Schema.Legacy}].[{StreetNameDetailConfiguration.TableName}] detail
                                            ON list.[StreetNameId] = detail.[StreetNameId]
                                    WHEN MATCHED THEN
                                        UPDATE
                                            SET list.[Status] = detail.[Status];");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                schema: "StreetNameRegistryLegacy",
                table: "StreetNameList");
        }
    }
}
