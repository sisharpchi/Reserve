using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Staffing.Application.StaffMembers.GetStaffMembers;

public sealed record GetStaffMembersQuery(Guid TenantId) : IQuery<IReadOnlyList<StaffMemberResponse>>;
