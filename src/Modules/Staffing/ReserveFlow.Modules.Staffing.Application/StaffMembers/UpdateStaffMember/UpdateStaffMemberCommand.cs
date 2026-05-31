using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Staffing.Application.StaffMembers.UpdateStaffMember;

public sealed record UpdateStaffMemberCommand(
    Guid StaffMemberId,
    string DisplayName,
    string? Email) : ICommand<StaffMemberResponse?>;
