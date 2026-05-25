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
        if (string.IsNullOrWhiteSpace(keycloakSubject))
        {
            throw new InvalidOperationException("Keycloak subject is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Email is required.");
        }

        return new User(Guid.NewGuid(), keycloakSubject, email, displayName);
    }
}
