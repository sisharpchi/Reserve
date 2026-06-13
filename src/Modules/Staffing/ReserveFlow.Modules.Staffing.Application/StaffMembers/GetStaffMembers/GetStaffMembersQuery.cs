using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;

namespace ReserveFlow.Modules.Staffing.Application.StaffMembers.GetStaffMembers;

public sealed record GetStaffMembersQuery(
    Guid TenantId,
    int? PageNumber,
    int? PageSize,
    string? Search,
    bool? IsActive,
    string? SortBy,
    string? SortDirection) : IQuery<PagedResponse<StaffMemberResponse>>;
