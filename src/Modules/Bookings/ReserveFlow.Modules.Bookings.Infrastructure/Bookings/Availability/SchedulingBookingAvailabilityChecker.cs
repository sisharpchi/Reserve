using Npgsql;
using ReserveFlow.Common.Application.Data;
using ReserveFlow.Modules.Bookings.Application.Bookings;

namespace ReserveFlow.Modules.Bookings.Infrastructure.Bookings.Availability;

internal sealed class SchedulingBookingAvailabilityChecker(
    IDatabaseConnectionStringProvider connectionStringProvider) : IBookingAvailabilityChecker
{
    private const string StaffAvailabilitySql = """
        select exists (
            select 1
            from scheduling.working_hours
            where tenant_id = @tenant_id
              and day_of_week = @day_of_week
              and staff_member_id = @target_id
              and starts_at <= @starts_at
              and ends_at >= @ends_at
        );
        """;

    private const string ResourceAvailabilitySql = """
        select exists (
            select 1
            from scheduling.working_hours
            where tenant_id = @tenant_id
              and day_of_week = @day_of_week
              and resource_id = @target_id
              and starts_at <= @starts_at
              and ends_at >= @ends_at
        );
        """;

    private const string StaffUnavailableSql = """
        select exists (
            select 1
            from scheduling.unavailable_periods
            where tenant_id = @tenant_id
              and staff_member_id = @target_id
              and starts_at_utc < @ends_at_utc
              and @starts_at_utc < ends_at_utc
        );
        """;

    private const string ResourceUnavailableSql = """
        select exists (
            select 1
            from scheduling.unavailable_periods
            where tenant_id = @tenant_id
              and resource_id = @target_id
              and starts_at_utc < @ends_at_utc
              and @starts_at_utc < ends_at_utc
        );
        """;

    public async Task<bool> IsAvailableAsync(
        Guid tenantId,
        Guid? staffMemberId,
        Guid? resourceId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty ||
            (staffMemberId is null && resourceId is null) ||
            endsAtUtc <= startsAtUtc ||
            startsAtUtc.UtcDateTime.Date != endsAtUtc.UtcDateTime.Date)
        {
            return false;
        }

        bool staffAvailable = staffMemberId is null ||
                              await HasCoveringWorkingHourAsync(
                                  StaffAvailabilitySql,
                                  tenantId,
                                  staffMemberId.Value,
                                  startsAtUtc,
                                  endsAtUtc,
                                  cancellationToken);

        bool resourceAvailable = resourceId is null ||
                                 await HasCoveringWorkingHourAsync(
                                     ResourceAvailabilitySql,
                                     tenantId,
                                     resourceId.Value,
                                     startsAtUtc,
                                     endsAtUtc,
                                     cancellationToken);

        bool staffUnavailable = staffMemberId is not null &&
                                await HasOverlappingUnavailablePeriodAsync(
                                    StaffUnavailableSql,
                                    tenantId,
                                    staffMemberId.Value,
                                    startsAtUtc,
                                    endsAtUtc,
                                    cancellationToken);

        bool resourceUnavailable = resourceId is not null &&
                                   await HasOverlappingUnavailablePeriodAsync(
                                       ResourceUnavailableSql,
                                       tenantId,
                                       resourceId.Value,
                                       startsAtUtc,
                                       endsAtUtc,
                                       cancellationToken);

        return staffAvailable && resourceAvailable && !staffUnavailable && !resourceUnavailable;
    }

    private async Task<bool> HasCoveringWorkingHourAsync(
        string sql,
        Guid tenantId,
        Guid targetId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionStringProvider.GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("tenant_id", tenantId);
        command.Parameters.AddWithValue("day_of_week", startsAtUtc.UtcDateTime.DayOfWeek.ToString());
        command.Parameters.AddWithValue("target_id", targetId);
        command.Parameters.AddWithValue("starts_at", TimeOnly.FromDateTime(startsAtUtc.UtcDateTime));
        command.Parameters.AddWithValue("ends_at", TimeOnly.FromDateTime(endsAtUtc.UtcDateTime));

        object? result = await command.ExecuteScalarAsync(cancellationToken);

        return result is bool isAvailable && isAvailable;
    }

    private async Task<bool> HasOverlappingUnavailablePeriodAsync(
        string sql,
        Guid tenantId,
        Guid targetId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionStringProvider.GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("tenant_id", tenantId);
        command.Parameters.AddWithValue("target_id", targetId);
        command.Parameters.AddWithValue("starts_at_utc", startsAtUtc);
        command.Parameters.AddWithValue("ends_at_utc", endsAtUtc);

        object? result = await command.ExecuteScalarAsync(cancellationToken);

        return result is bool hasOverlap && hasOverlap;
    }
}
