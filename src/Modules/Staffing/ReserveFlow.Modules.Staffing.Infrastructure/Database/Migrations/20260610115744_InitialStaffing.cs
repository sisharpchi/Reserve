using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReserveFlow.Modules.Staffing.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class InitialStaffing : Migration
{
    private static readonly string[] TenantActiveColumns = ["tenant_id", "is_active"];
    private static readonly string[] TenantEmailColumns = ["tenant_id", "email"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "staffing");

        migrationBuilder.CreateTable(
            name: "staff_members",
            schema: "staffing",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_staff_members", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_staff_members_tenant_active",
            schema: "staffing",
            table: "staff_members",
            columns: TenantActiveColumns);

        migrationBuilder.CreateIndex(
            name: "ix_staff_members_tenant_email",
            schema: "staffing",
            table: "staff_members",
            columns: TenantEmailColumns);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "staff_members",
            schema: "staffing");
    }
}
