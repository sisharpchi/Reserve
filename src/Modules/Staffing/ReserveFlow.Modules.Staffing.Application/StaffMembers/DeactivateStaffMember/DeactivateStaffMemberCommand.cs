using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Staffing.Application.StaffMembers.DeactivateStaffMember;

public sealed record DeactivateStaffMemberCommand(Guid StaffMemberId) : ICommand<StaffMemberResponse?>;
