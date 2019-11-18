using Microsoft.EntityFrameworkCore.Migrations;

namespace StreetNameRegistry.Projections.Legacy.Migrations
{
    public partial class AddValidCountView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
            CREATE VIEW [{Infrastructure.Schema.Legacy}].[{LegacyContext.StreetNameListViewCountName}]
            WITH SCHEMABINDING
            AS
            SELECT COUNT_BIG(*) as Count
            FROM [{Infrastructure.Schema.Legacy}].[{StreetNameList.StreetNameListConfiguration.TableName}]
            WHERE [Complete] = 1 AND [Removed] = 0 AND [PersistentLocalId] is not null");

            migrationBuilder.Sql($@"CREATE UNIQUE CLUSTERED INDEX IX_{LegacyContext.StreetNameListViewCountName} ON [{Infrastructure.Schema.Legacy}].[{LegacyContext.StreetNameListViewCountName}] (Count)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"DROP INDEX [IX_{LegacyContext.StreetNameListViewCountName}] ON [{Infrastructure.Schema.Legacy}].[{LegacyContext.StreetNameListViewCountName}]");
            migrationBuilder.Sql($@"DROP VIEW [{Infrastructure.Schema.Legacy}].[{LegacyContext.StreetNameListViewCountName}]");
        }
    }
}
