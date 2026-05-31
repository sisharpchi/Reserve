using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Identity.Application.TenantUsers.AssignTenantOwner;

public sealed record AssignTenantOwnerCommand(
    Guid TenantId,
    string KeycloakSubject,
    string Email,
    string? DisplayName) : ICommand<AssignTenantOwnerResponse>;
