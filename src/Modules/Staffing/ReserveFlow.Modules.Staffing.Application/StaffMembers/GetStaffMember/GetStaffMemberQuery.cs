using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Staffing.Application.StaffMembers.GetStaffMember;

public sealed record GetStaffMemberQuery(Guid StaffMemberId) : IQuery<StaffMemberResponse?>;
