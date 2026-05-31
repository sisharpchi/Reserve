using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Identity.Domain.Users;

public sealed class User : Entity
{
    private User(
        Guid id,
        string keycloakSubject,
        string email,
        string displayName)
        : base(id)
    {
        KeycloakSubject = keycloakSubject;
        Email = email;
        DisplayName = displayName;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private User()
    {
    }

    public string KeycloakSubject { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public static User Create(string keycloakSubject, string email, string displayName)
    {
        string normalizedSubject = NormalizeRequired(keycloakSubject, "Keycloak subject");
        string normalizedEmail = NormalizeRequired(email, "Email").ToLowerInvariant();
        string normalizedDisplayName = NormalizeRequired(displayName, "Display name");

        return new User(Guid.NewGuid(), normalizedSubject, normalizedEmail, normalizedDisplayName);
    }

    public void UpdateProfile(string email, string displayName)
    {
        Email = NormalizeRequired(email, "Email").ToLowerInvariant();
        DisplayName = NormalizeRequired(displayName, "Display name");
    }

    private static string NormalizeRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return value.Trim();
    }
}
