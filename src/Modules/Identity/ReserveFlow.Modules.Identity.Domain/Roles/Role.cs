using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Identity.Domain.Roles;

public sealed class Role : Entity
{
    private Role(
        Guid id,
        string name,
        string displayName,
        bool isPlatformRole)
        : base(id)
    {
        Name = name;
        DisplayName = displayName;
        IsPlatformRole = isPlatformRole;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private Role()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public bool IsPlatformRole { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static Role Create(string name, string displayName, bool isPlatformRole)
    {
        string normalizedName = NormalizeRequired(name, "Role name");
        string normalizedDisplayName = NormalizeRequired(displayName, "Role display name");

        return new Role(Guid.NewGuid(), normalizedName, normalizedDisplayName, isPlatformRole);
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
