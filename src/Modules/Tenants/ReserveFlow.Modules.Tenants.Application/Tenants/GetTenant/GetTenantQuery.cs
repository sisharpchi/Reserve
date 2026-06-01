using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Tenants.Application.Tenants.GetTenant;

public sealed record GetTenantQuery(Guid TenantId) : IQuery<TenantResponse?>;
