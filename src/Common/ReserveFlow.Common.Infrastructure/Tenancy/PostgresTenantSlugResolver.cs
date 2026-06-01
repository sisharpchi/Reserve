using Npgsql;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Data;

namespace ReserveFlow.Common.Infrastructure.Tenancy;

internal sealed class PostgresTenantSlugResolver(
    IDatabaseConnectionStringProvider connectionStringProvider) : ITenantSlugResolver
{
    private const string ResolveTenantIdSql = """
        select id
        from platform.tenants
        where slug = @tenant_slug
        limit 1;
        """;

    public async Task<Guid?> ResolveTenantIdAsync(
        string tenantSlug,
        CancellationToken cancellationToken = default)
    {
        string normalizedSlug = tenantSlug.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(normalizedSlug))
        {
            return null;
        }

        await using var connection = new NpgsqlConnection(connectionStringProvider.GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(ResolveTenantIdSql, connection);
        command.Parameters.AddWithValue("tenant_slug", normalizedSlug);

        object? result = await command.ExecuteScalarAsync(cancellationToken);

        return result is Guid tenantId ? tenantId : null;
    }
}
