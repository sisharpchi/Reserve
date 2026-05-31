using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Tenants.Application.Tenants.GetPublicTenants;

public sealed record GetPublicTenantsQuery(
    Guid? CategoryId,
    string? Search) : IQuery<IReadOnlyList<TenantResponse>>;
