using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Identity.Domain.Permissions;

public sealed class Permission : Entity
{
    private Permission(
        Guid id,
        string code,
        string description)
        : base(id)
    {
        Code = code;
        Description = description;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private Permission()
    {
    }

    public string Code { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public static Permission Create(string code, string description)
    {
        string normalizedCode = NormalizeRequired(code, "Permission code");
        string normalizedDescription = NormalizeRequired(description, "Permission description");

        return new Permission(Guid.NewGuid(), normalizedCode, normalizedDescription);
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
