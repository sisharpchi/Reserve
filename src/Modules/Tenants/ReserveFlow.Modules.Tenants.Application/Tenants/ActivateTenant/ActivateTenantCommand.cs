using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Tenants.Application.Tenants.ActivateTenant;

public sealed record ActivateTenantCommand(Guid TenantId) : ICommand<TenantResponse?>;
