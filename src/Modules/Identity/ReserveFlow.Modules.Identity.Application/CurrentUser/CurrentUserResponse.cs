namespace ReserveFlow.Modules.Identity.Application.CurrentUser;

public sealed record CurrentUserResponse(
    bool IsAuthenticated,
    Guid? UserId,
    string? KeycloakSubject,
    string? Email,
    IReadOnlyCollection<string> Permissions,
    CurrentTenantResponse Tenant);
