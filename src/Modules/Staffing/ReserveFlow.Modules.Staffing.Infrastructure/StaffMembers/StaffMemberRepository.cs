using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Staffing.Application.StaffMembers;
using ReserveFlow.Modules.Staffing.Domain.StaffMembers;
using ReserveFlow.Modules.Staffing.Infrastructure.Database;

namespace ReserveFlow.Modules.Staffing.Infrastructure.StaffMembers;

internal sealed class StaffMemberRepository(StaffingDbContext dbContext) : IStaffMemberRepository
{
    public void Insert(StaffMember staffMember)
    {
        dbContext.StaffMembers.Add(staffMember);
    }

    public async Task<StaffMember?> GetByIdAsync(Guid staffMemberId, CancellationToken cancellationToken = default)
    {
        return await dbContext.StaffMembers.FirstOrDefaultAsync(
            staffMember => staffMember.Id == staffMemberId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<StaffMember>> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.StaffMembers
            .Where(staffMember => staffMember.TenantId == tenantId)
            .OrderBy(staffMember => staffMember.DisplayName)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StaffMember>> GetActiveByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.StaffMembers
            .Where(staffMember => staffMember.TenantId == tenantId && staffMember.IsActive)
            .OrderBy(staffMember => staffMember.DisplayName)
            .ToArrayAsync(cancellationToken);
    }
}
