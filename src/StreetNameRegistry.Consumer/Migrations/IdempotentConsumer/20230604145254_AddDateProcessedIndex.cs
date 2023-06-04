using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreetNameRegistry.Consumer.Migrations.IdempotentConsumer
{
    public partial class AddDateProcessedIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProcessedMessages_DateProcessed",
                schema: "StreetNameRegistryConsumerMunicipality",
                table: "ProcessedMessages",
                column: "DateProcessed");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProcessedMessages_DateProcessed",
                schema: "StreetNameRegistryConsumerMunicipality",
                table: "ProcessedMessages");
        }
    }
}
