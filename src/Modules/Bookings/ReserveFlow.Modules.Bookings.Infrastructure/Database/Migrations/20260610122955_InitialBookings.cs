using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReserveFlow.Modules.Bookings.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class InitialBookings : Migration
{
    private static readonly string[] BookingHistoryColumns = ["tenant_id", "booking_id", "changed_at_utc"];
    private static readonly string[] BookingResourceTimeColumns = ["tenant_id", "resource_id", "starts_at_utc", "ends_at_utc"];
    private static readonly string[] BookingStaffTimeColumns = ["tenant_id", "staff_member_id", "starts_at_utc", "ends_at_utc"];
    private static readonly string[] BookingStatusTimeColumns = ["tenant_id", "status", "starts_at_utc"];
    private static readonly string[] BookingPublicLookupColumns = ["public_reference", "access_token"];
    private static readonly string[] BookingIdempotencyColumns = ["tenant_id", "idempotency_key"];
    private static readonly string[] CustomerTenantEmailColumns = ["tenant_id", "email"];
    private static readonly string[] OutboxTenantOccurredColumns = ["tenant_id", "occurred_on_utc"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "bookings");

        migrationBuilder.Sql(
            """
            CREATE EXTENSION IF NOT EXISTS btree_gist;
            """);

        migrationBuilder.CreateTable(
            name: "booking_history",
            schema: "bookings",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                changed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_booking_history", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "booking_policies",
            schema: "bookings",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                minimum_advance_minutes = table.Column<int>(type: "integer", nullable: false),
                cancellation_deadline_hours = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_booking_policies", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "bookings",
            schema: "bookings",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                service_id = table.Column<Guid>(type: "uuid", nullable: false),
                staff_member_id = table.Column<Guid>(type: "uuid", nullable: true),
                resource_id = table.Column<Guid>(type: "uuid", nullable: true),
                starts_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ends_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                public_reference = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                access_token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                concurrency_token = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_bookings", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "customers",
            schema: "bookings",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                phone_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                registered_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_customers", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "outbox_messages",
            schema: "bookings",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                payload = table.Column<string>(type: "jsonb", nullable: false),
                processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                error = table.Column<string>(type: "text", nullable: true),
                retry_count = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_outbox_messages", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_booking_history_tenant_booking_time",
            schema: "bookings",
            table: "booking_history",
            columns: BookingHistoryColumns);

        migrationBuilder.CreateIndex(
            name: "ux_booking_policies_tenant",
            schema: "bookings",
            table: "booking_policies",
            column: "tenant_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_bookings_tenant_resource_time",
            schema: "bookings",
            table: "bookings",
            columns: BookingResourceTimeColumns);

        migrationBuilder.CreateIndex(
            name: "ix_bookings_tenant_staff_time",
            schema: "bookings",
            table: "bookings",
            columns: BookingStaffTimeColumns);

        migrationBuilder.CreateIndex(
            name: "ix_bookings_tenant_status_time",
            schema: "bookings",
            table: "bookings",
            columns: BookingStatusTimeColumns);

        migrationBuilder.CreateIndex(
            name: "ux_bookings_public_reference_access_token",
            schema: "bookings",
            table: "bookings",
            columns: BookingPublicLookupColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_bookings_tenant_idempotency_key",
            schema: "bookings",
            table: "bookings",
            columns: BookingIdempotencyColumns,
            unique: true,
            filter: "idempotency_key IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ux_customers_tenant_email",
            schema: "bookings",
            table: "customers",
            columns: CustomerTenantEmailColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_outbox_messages_processed_on_utc",
            schema: "bookings",
            table: "outbox_messages",
            column: "processed_on_utc");

        migrationBuilder.CreateIndex(
            name: "IX_outbox_messages_tenant_id_occurred_on_utc",
            schema: "bookings",
            table: "outbox_messages",
            columns: OutboxTenantOccurredColumns);

        migrationBuilder.Sql(
            """
            ALTER TABLE bookings.bookings
            ADD CONSTRAINT ex_bookings_no_active_staff_overlap
            EXCLUDE USING gist (
                tenant_id WITH =,
                staff_member_id WITH =,
                tstzrange(starts_at_utc, ends_at_utc, '[)') WITH &&
            )
            WHERE (staff_member_id IS NOT NULL AND status IN ('Pending', 'Confirmed', 'Rescheduled'));
            """);

        migrationBuilder.Sql(
            """
            ALTER TABLE bookings.bookings
            ADD CONSTRAINT ex_bookings_no_active_resource_overlap
            EXCLUDE USING gist (
                tenant_id WITH =,
                resource_id WITH =,
                tstzrange(starts_at_utc, ends_at_utc, '[)') WITH &&
            )
            WHERE (resource_id IS NOT NULL AND status IN ('Pending', 'Confirmed', 'Rescheduled'));
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "booking_history",
            schema: "bookings");

        migrationBuilder.DropTable(
            name: "booking_policies",
            schema: "bookings");

        migrationBuilder.DropTable(
            name: "bookings",
            schema: "bookings");

        migrationBuilder.DropTable(
            name: "customers",
            schema: "bookings");

        migrationBuilder.DropTable(
            name: "outbox_messages",
            schema: "bookings");
    }
}
