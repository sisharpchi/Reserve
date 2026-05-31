namespace ReserveFlow.Modules.Identity.Presentation;

public sealed record AssignTenantOwnerRequest(
    string KeycloakSubject,
    string Email,
    string? DisplayName);
