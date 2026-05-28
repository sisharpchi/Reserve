using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Tenants.Application.Tenants.SuspendTenant;

public sealed record SuspendTenantCommand(Guid TenantId) : ICommand<TenantResponse?>;
