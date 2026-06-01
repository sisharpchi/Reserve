using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Tenants.Application.Tenants.GetTenant;

public sealed class GetTenantQueryHandler(ITenantRepository tenantRepository)
    : IQueryHandler<GetTenantQuery, TenantResponse?>
{
    public async Task<TenantResponse?> Handle(
        GetTenantQuery query,
        CancellationToken cancellationToken = default)
    {
        var tenant = await tenantRepository.GetByIdAsync(query.TenantId, cancellationToken);

        return tenant is null ? null : TenantResponse.FromTenant(tenant);
    }
}
