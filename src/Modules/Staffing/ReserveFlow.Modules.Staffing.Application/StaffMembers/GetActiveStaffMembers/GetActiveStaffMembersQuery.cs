using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Staffing.Application.StaffMembers;

namespace ReserveFlow.Modules.Staffing.Application.StaffMembers.GetActiveStaffMembers;

public sealed record GetActiveStaffMembersQuery(Guid TenantId) : IQuery<IReadOnlyList<StaffMemberResponse>>;
