using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using ReserveFlow.Modules.Bookings.Infrastructure.Database;
using Testcontainers.PostgreSql;

namespace ReserveFlow.IntegrationTests.Bookings;

public sealed class BookingMigrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("reserveflow_integration_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    [Fact]
    public async Task InitialMigrationBlocksOverlappingActiveStaffAndResourceBookings()
    {
        await using BookingsDbContext dbContext = CreateDbContext();

        await dbContext.Database.MigrateAsync();

        Guid tenantId = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();
        Guid serviceId = Guid.NewGuid();
        Guid staffMemberId = Guid.NewGuid();
        Guid resourceId = Guid.NewGuid();

        await InsertBookingAsync(
            dbContext,
            tenantId,
            customerId,
            serviceId,
            staffMemberId,
            resourceId: null,
            startsAtUtc: new DateTimeOffset(2026, 6, 1, 9, 0, 0, TimeSpan.Zero),
            endsAtUtc: new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero),
            publicReference: "RFSTAFF1",
            accessToken: "staff-token-1");

        PostgresException staffException = await Assert.ThrowsAsync<PostgresException>(() => InsertBookingAsync(
            dbContext,
            tenantId,
            customerId,
            serviceId,
            staffMemberId,
            resourceId: null,
            startsAtUtc: new DateTimeOffset(2026, 6, 1, 9, 30, 0, TimeSpan.Zero),
            endsAtUtc: new DateTimeOffset(2026, 6, 1, 10, 30, 0, TimeSpan.Zero),
            publicReference: "RFSTAFF2",
            accessToken: "staff-token-2"));

        Assert.Equal("ex_bookings_no_active_staff_overlap", staffException.ConstraintName);

        await InsertBookingAsync(
            dbContext,
            tenantId,
            customerId,
            serviceId,
            staffMemberId: null,
            resourceId,
            startsAtUtc: new DateTimeOffset(2026, 6, 1, 11, 0, 0, TimeSpan.Zero),
            endsAtUtc: new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero),
            publicReference: "RFRESOURCE1",
            accessToken: "resource-token-1");

        PostgresException resourceException = await Assert.ThrowsAsync<PostgresException>(() => InsertBookingAsync(
            dbContext,
            tenantId,
            customerId,
            serviceId,
            staffMemberId: null,
            resourceId,
            startsAtUtc: new DateTimeOffset(2026, 6, 1, 11, 30, 0, TimeSpan.Zero),
            endsAtUtc: new DateTimeOffset(2026, 6, 1, 12, 30, 0, TimeSpan.Zero),
            publicReference: "RFRESOURCE2",
            accessToken: "resource-token-2"));

        Assert.Equal("ex_bookings_no_active_resource_overlap", resourceException.ConstraintName);
    }

    public Task InitializeAsync()
    {
        return _postgres.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _postgres.DisposeAsync().AsTask();
    }

    private BookingsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseNpgsql(
                _postgres.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName,
                    "bookings"))
            .Options;

        return new BookingsDbContext(options);
    }

    private static Task<int> InsertBookingAsync(
        BookingsDbContext dbContext,
        Guid tenantId,
        Guid customerId,
        Guid serviceId,
        Guid? staffMemberId,
        Guid? resourceId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        string publicReference,
        string accessToken)
    {
        return dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
             INSERT INTO bookings.bookings
             (id, tenant_id, customer_id, service_id, staff_member_id, resource_id, starts_at_utc, ends_at_utc, status, public_reference, access_token, concurrency_token)
             VALUES
             ({Guid.NewGuid()}, {tenantId}, {customerId}, {serviceId}, {staffMemberId}, {resourceId}, {startsAtUtc}, {endsAtUtc}, {"Confirmed"}, {publicReference}, {accessToken}, {Guid.NewGuid()});
             """);
    }
}
