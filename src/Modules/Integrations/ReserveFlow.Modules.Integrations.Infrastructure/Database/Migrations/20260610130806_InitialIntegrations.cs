using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReserveFlow.Modules.Integrations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialIntegrations : Migration
    {
        private static readonly string[] TenantReceivedColumns = ["tenant_id", "received_at_utc"];
        private static readonly string[] SourceExternalMessageColumns = ["source", "external_message_id"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "integrations");

            migrationBuilder.CreateTable(
                name: "webhook_inbox_messages",
                schema: "integrations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    external_message_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    event_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    received_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webhook_inbox_messages", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_webhook_inbox_messages_tenant_received",
                schema: "integrations",
                table: "webhook_inbox_messages",
                columns: TenantReceivedColumns);

            migrationBuilder.CreateIndex(
                name: "ux_webhook_inbox_messages_source_external_id",
                schema: "integrations",
                table: "webhook_inbox_messages",
                columns: SourceExternalMessageColumns,
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "webhook_inbox_messages",
                schema: "integrations");
        }
    }
}
