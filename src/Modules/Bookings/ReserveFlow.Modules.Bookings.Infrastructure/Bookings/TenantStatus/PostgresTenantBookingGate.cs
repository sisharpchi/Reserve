using Npgsql;
using ReserveFlow.Common.Application.Data;
using ReserveFlow.Modules.Bookings.Application.Bookings;

namespace ReserveFlow.Modules.Bookings.Infrastructure.Bookings.TenantStatus;

internal sealed class PostgresTenantBookingGate(
    IDatabaseConnectionStringProvider connectionStringProvider) : ITenantBookingGate
{
    private const string CanAcceptPublicBookingSql = """
        select exists (
            select 1
            from platform.tenants
            where id = @tenant_id
              and status = 'Active'
        );
        """;

    public async Task<bool> CanAcceptPublicBookingAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
        {
            return false;
        }

        await using var connection = new NpgsqlConnection(connectionStringProvider.GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(CanAcceptPublicBookingSql, connection);
        command.Parameters.AddWithValue("tenant_id", tenantId);

        object? result = await command.ExecuteScalarAsync(cancellationToken);

        return result is bool canAcceptPublicBooking && canAcceptPublicBooking;
    }
}
