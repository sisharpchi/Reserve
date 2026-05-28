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

        var staffMember = new StaffMember(
            Guid.NewGuid(),
            tenantId,
            normalizedDisplayName,
            NormalizeEmail(email));

        staffMember.RaiseDomainEvent(new StaffMemberCreatedDomainEvent(
            staffMember.Id,
            staffMember.TenantId,
            staffMember.DisplayName));

        return staffMember;
    }

    public void Update(string displayName, string? email)
    {
        DisplayName = NormalizeRequired(displayName, "Staff display name");
        Email = NormalizeEmail(email);

        RaiseDomainEvent(new StaffMemberUpdatedDomainEvent(Id, TenantId, DisplayName));
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Staff member is already inactive.");
        }

        IsActive = false;
        RaiseDomainEvent(new StaffMemberDeactivatedDomainEvent(Id, TenantId));
    }

    private static string NormalizeRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return value.Trim();
    }

    private static string? NormalizeEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email)
            ? null
            : email.Trim().ToLowerInvariant();
    }
}
