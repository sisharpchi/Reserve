using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReserveFlow.Modules.Notifications.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class InitialNotifications : Migration
{
    private static readonly string[] StatusDeliverAtColumns = ["status", "deliver_at_utc"];
    private static readonly string[] TenantCorrelationStatusColumns = ["tenant_id", "correlation_key", "status"];
    private static readonly string[] TenantStatusCreatedColumns = ["tenant_id", "status", "created_at_utc"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "notifications");

        migrationBuilder.CreateTable(
            name: "notification_messages",
            schema: "notifications",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                recipient = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                subject = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                body = table.Column<string>(type: "text", nullable: false),
                status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                deliver_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                correlation_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                sent_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                error = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_notification_messages", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_notification_messages_status_deliver_at",
            schema: "notifications",
            table: "notification_messages",
            columns: StatusDeliverAtColumns);

        migrationBuilder.CreateIndex(
            name: "ix_notification_messages_tenant_correlation_status",
            schema: "notifications",
            table: "notification_messages",
            columns: TenantCorrelationStatusColumns);

        migrationBuilder.CreateIndex(
            name: "ix_notification_messages_tenant_status_created",
            schema: "notifications",
            table: "notification_messages",
            columns: TenantStatusCreatedColumns);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "notification_messages",
            schema: "notifications");
    }
}
