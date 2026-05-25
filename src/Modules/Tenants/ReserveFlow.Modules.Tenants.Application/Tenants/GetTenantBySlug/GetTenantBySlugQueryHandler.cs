using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Tenants.Domain.Tenants;

namespace ReserveFlow.Modules.Tenants.Application.Tenants.GetTenantBySlug;

public sealed class GetTenantBySlugQueryHandler(ITenantRepository tenantRepository)
    : IQueryHandler<GetTenantBySlugQuery, TenantResponse?>
{
    public async Task<TenantResponse?> Handle(
        GetTenantBySlugQuery query,
        CancellationToken cancellationToken = default)
    {
        string normalizedSlug = Tenant.NormalizeSlug(query.Slug);
        Tenant? tenant = await tenantRepository.GetBySlugAsync(normalizedSlug, cancellationToken);

        return tenant is null ? null : TenantResponse.FromTenant(tenant);
    }
}
