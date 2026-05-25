using ReserveFlow.Modules.Staffing.Domain.StaffMembers;

namespace ReserveFlow.Modules.Staffing.Application.StaffMembers;

public sealed record StaffMemberResponse(
    Guid Id,
    Guid TenantId,
    string DisplayName,
    string? Email,
    bool IsActive)
{
    public static StaffMemberResponse FromStaffMember(StaffMember staffMember)
    {
        return new StaffMemberResponse(
            staffMember.Id,
            staffMember.TenantId,
            staffMember.DisplayName,
            staffMember.Email,
            staffMember.IsActive);
    }
}
