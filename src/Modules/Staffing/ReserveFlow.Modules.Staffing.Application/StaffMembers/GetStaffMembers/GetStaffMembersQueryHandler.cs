using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Modules.Staffing.Domain.StaffMembers;

namespace ReserveFlow.Modules.Staffing.Application.StaffMembers.GetStaffMembers;

public sealed class GetStaffMembersQueryHandler(IStaffMemberRepository staffMemberRepository)
    : IQueryHandler<GetStaffMembersQuery, PagedResponse<StaffMemberResponse>>
{
    public async Task<PagedResponse<StaffMemberResponse>> Handle(
        GetStaffMembersQuery query,
        CancellationToken cancellationToken = default)
    {
        PageRequest pageRequest = PageRequest.Create(query.PageNumber, query.PageSize);
        PagedResult<StaffMember> staffMembers = await staffMemberRepository.GetByTenantIdAsync(
            query.TenantId,
            pageRequest,
            query.Search,
            query.IsActive,
            query.SortBy,
            query.SortDirection,
            cancellationToken);

        StaffMemberResponse[] items = staffMembers.Items
            .Select(StaffMemberResponse.FromStaffMember)
            .ToArray();

        return new PagedResponse<StaffMemberResponse>(
            items,
            pageRequest.PageNumber,
            pageRequest.PageSize,
            staffMembers.TotalCount);
    }
}
