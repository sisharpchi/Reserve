using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReserveFlow.Modules.Audit.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class InitialAudit : Migration
{
    private static readonly string[] TenantOccurredColumns = ["tenant_id", "occurred_on_utc"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "audit");

        migrationBuilder.CreateTable(
            name: "audit_logs",
            schema: "audit",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                user_id = table.Column<Guid>(type: "uuid", nullable: true),
                action = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                entity_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                details_json = table.Column<string>(type: "jsonb", nullable: false),
                occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_audit_logs", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_tenant_occurred",
            schema: "audit",
            table: "audit_logs",
            columns: TenantOccurredColumns);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "audit_logs",
            schema: "audit");
    }
}
