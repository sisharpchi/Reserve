namespace ReserveFlow.Common.Application.Abstractions;

public interface ITenantSlugResolver
{
    Task<Guid?> ResolveTenantIdAsync(
        string tenantSlug,
        CancellationToken cancellationToken = default);
}
