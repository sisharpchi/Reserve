namespace ReserveFlow.Modules.Identity.Application.Keycloak;

public sealed record KeycloakProvisionedUser(
    string KeycloakSubject,
    string Email,
    string DisplayName,
    bool CreatedInKeycloak,
    bool InvitationEmailSent);
