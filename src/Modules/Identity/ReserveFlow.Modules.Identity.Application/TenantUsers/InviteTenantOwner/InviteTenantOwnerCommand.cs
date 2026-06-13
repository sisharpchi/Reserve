using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Identity.Application.TenantUsers.InviteTenantOwner;

public sealed record InviteTenantOwnerCommand(
    Guid TenantId,
    string Email,
    string? DisplayName) : ICommand<InviteTenantOwnerResponse>;
