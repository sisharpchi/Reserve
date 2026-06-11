namespace ReserveFlow.Modules.Identity.Infrastructure.Keycloak;

internal sealed class KeycloakAdminOptions
{
    public const string SectionName = "Keycloak:Admin";

    public string BaseUrl { get; set; } = "http://localhost:18080";

    public string Realm { get; set; } = "reserveflow";

    public string ClientId { get; set; } = "reserveflow-admin";

    public string ClientSecret { get; set; } = string.Empty;

    public string TenantAdminRealmRole { get; set; } = "tenant-admin";

    public bool SendInvitationEmail { get; set; }

    public string? InvitationClientId { get; set; } = "reserveflow-web";

    public string? InvitationRedirectUri { get; set; } = "http://localhost:4200";

    public int InvitationLifespanSeconds { get; set; } = 86_400;

    public string[] RequiredActions { get; set; } = ["VERIFY_EMAIL", "UPDATE_PASSWORD"];
}
