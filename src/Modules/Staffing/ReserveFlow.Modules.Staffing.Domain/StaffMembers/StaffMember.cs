using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Staffing.Domain.StaffMembers;

public sealed class StaffMember : Entity
{
    private StaffMember(
        Guid id,
        Guid tenantId,
        string displayName,
        string? email)
        : base(id)
    {
        TenantId = tenantId;
        DisplayName = displayName;
        Email = email;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private StaffMember()
    {
    }

    public Guid TenantId { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static StaffMember Create(Guid tenantId, string displayName, string? email)
    {
        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException("Tenant id is required.");
        }

        string normalizedDisplayName = NormalizeRequired(displayName, "Staff display name");
        string? normalizedEmail = string.IsNullOrWhiteSpace(email)
            ? null
            : email.Trim().ToLowerInvariant();

        var staffMember = new StaffMember(
            Guid.NewGuid(),
            tenantId,
            normalizedDisplayName,
            normalizedEmail);

        staffMember.RaiseDomainEvent(new StaffMemberCreatedDomainEvent(
            staffMember.Id,
            staffMember.TenantId,
            staffMember.DisplayName));

        return staffMember;
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
