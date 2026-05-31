namespace ReserveFlow.Modules.Identity.Application.TenantUsers.AssignTenantOwner;

public sealed record AssignTenantOwnerResponse(
    Guid TenantId,
    Guid UserId,
    string KeycloakSubject,
    string Email,
    string DisplayName,
    string Role,
    bool UserCreated,
    bool MembershipCreated);
