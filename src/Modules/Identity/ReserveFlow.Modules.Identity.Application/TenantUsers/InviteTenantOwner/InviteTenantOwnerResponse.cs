namespace ReserveFlow.Modules.Identity.Application.TenantUsers.InviteTenantOwner;

public sealed record InviteTenantOwnerResponse(
    Guid TenantId,
    Guid UserId,
    string KeycloakSubject,
    string Email,
    string DisplayName,
    string Role,
    bool CreatedInKeycloak,
    bool InvitationEmailSent,
    bool LocalUserCreated,
    bool MembershipCreated);
