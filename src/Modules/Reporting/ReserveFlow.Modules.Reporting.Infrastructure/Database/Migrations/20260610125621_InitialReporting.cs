using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReserveFlow.Modules.Reporting.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialReporting : Migration
    {
        private static readonly string[] TenantDateColumns = ["tenant_id", "date"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reporting");

            migrationBuilder.CreateTable(
                name: "daily_booking_reports",
                schema: "reporting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    created_bookings = table.Column<int>(type: "integer", nullable: false),
                    cancelled_bookings = table.Column<int>(type: "integer", nullable: false),
                    completed_bookings = table.Column<int>(type: "integer", nullable: false),
                    no_show_bookings = table.Column<int>(type: "integer", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_booking_reports", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_daily_booking_reports_tenant_date",
                schema: "reporting",
                table: "daily_booking_reports",
                columns: TenantDateColumns,
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_booking_reports",
                schema: "reporting");
        }
    }
}
