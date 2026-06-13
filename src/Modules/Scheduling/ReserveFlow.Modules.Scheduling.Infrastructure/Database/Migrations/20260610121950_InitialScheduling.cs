using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReserveFlow.Modules.Scheduling.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class InitialScheduling : Migration
{
    private static readonly string[] TenantResourceTimeColumns = ["tenant_id", "resource_id", "starts_at_utc", "ends_at_utc"];
    private static readonly string[] TenantStaffTimeColumns = ["tenant_id", "staff_member_id", "starts_at_utc", "ends_at_utc"];
    private static readonly string[] TenantResourceDayColumns = ["tenant_id", "resource_id", "day_of_week"];
    private static readonly string[] TenantStaffDayColumns = ["tenant_id", "staff_member_id", "day_of_week"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "scheduling");

        migrationBuilder.CreateTable(
            name: "unavailable_periods",
            schema: "scheduling",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                staff_member_id = table.Column<Guid>(type: "uuid", nullable: true),
                resource_id = table.Column<Guid>(type: "uuid", nullable: true),
                starts_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ends_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_unavailable_periods", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "working_hours",
            schema: "scheduling",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                staff_member_id = table.Column<Guid>(type: "uuid", nullable: true),
                resource_id = table.Column<Guid>(type: "uuid", nullable: true),
                day_of_week = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                starts_at = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                ends_at = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_working_hours", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_unavailable_periods_tenant_resource_time",
            schema: "scheduling",
            table: "unavailable_periods",
            columns: TenantResourceTimeColumns);

        migrationBuilder.CreateIndex(
            name: "ix_unavailable_periods_tenant_staff_time",
            schema: "scheduling",
            table: "unavailable_periods",
            columns: TenantStaffTimeColumns);

        migrationBuilder.CreateIndex(
            name: "ix_working_hours_tenant_resource_day",
            schema: "scheduling",
            table: "working_hours",
            columns: TenantResourceDayColumns);

        migrationBuilder.CreateIndex(
            name: "ix_working_hours_tenant_staff_day",
            schema: "scheduling",
            table: "working_hours",
            columns: TenantStaffDayColumns);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "unavailable_periods",
            schema: "scheduling");

        migrationBuilder.DropTable(
            name: "working_hours",
            schema: "scheduling");
    }
}
