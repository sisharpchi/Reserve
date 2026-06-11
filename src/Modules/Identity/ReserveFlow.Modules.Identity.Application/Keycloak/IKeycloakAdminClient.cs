namespace ReserveFlow.Modules.Identity.Application.Keycloak;

public interface IKeycloakAdminClient
{
    Task<KeycloakProvisionedUser> ProvisionTenantAdminAsync(
        string email,
        string? displayName,
        CancellationToken cancellationToken = default);
}
