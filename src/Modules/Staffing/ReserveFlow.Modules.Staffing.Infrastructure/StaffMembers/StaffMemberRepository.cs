using Microsoft.EntityFrameworkCore;
using ReserveFlow.Common.Application.Pagination;
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

    public async Task<PagedResult<StaffMember>> GetByTenantIdAsync(
        Guid tenantId,
        PageRequest pageRequest,
        string? search,
        bool? isActive,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default)
    {
        IQueryable<StaffMember> query = dbContext.StaffMembers
            .Where(staffMember => staffMember.TenantId == tenantId);

        query = ApplySearch(query, search);
        query = ApplyActiveFilter(query, isActive);

        int totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, sortBy, sortDirection);

        StaffMember[] items = await query
            .Skip(pageRequest.Skip)
            .Take(pageRequest.PageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<StaffMember>(items, totalCount);
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

    private static IQueryable<StaffMember> ApplySearch(IQueryable<StaffMember> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        string searchPattern = $"%{search.Trim()}%";

        return query.Where(staffMember =>
            EF.Functions.ILike(staffMember.DisplayName, searchPattern) ||
            (staffMember.Email != null && EF.Functions.ILike(staffMember.Email, searchPattern)));
    }

    private static IQueryable<StaffMember> ApplyActiveFilter(IQueryable<StaffMember> query, bool? isActive)
    {
        return isActive.HasValue
            ? query.Where(staffMember => staffMember.IsActive == isActive.Value)
            : query;
    }

    private static IOrderedQueryable<StaffMember> ApplySorting(
        IQueryable<StaffMember> query,
        string? sortBy,
        string? sortDirection)
    {
        bool descending = IsDescending(sortDirection);

        return NormalizeSortKey(sortBy) switch
        {
            "email" => descending
                ? query.OrderByDescending(staffMember => staffMember.Email).ThenBy(staffMember => staffMember.DisplayName)
                : query.OrderBy(staffMember => staffMember.Email).ThenBy(staffMember => staffMember.DisplayName),
            "createdat" or "createdatutc" => descending
                ? query.OrderByDescending(staffMember => staffMember.CreatedAtUtc).ThenBy(staffMember => staffMember.DisplayName)
                : query.OrderBy(staffMember => staffMember.CreatedAtUtc).ThenBy(staffMember => staffMember.DisplayName),
            _ => descending
                ? query.OrderByDescending(staffMember => staffMember.DisplayName)
                : query.OrderBy(staffMember => staffMember.DisplayName)
        };
    }

    private static bool IsDescending(string? sortDirection)
    {
        return string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sortDirection, "descending", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeSortKey(string? sortBy)
    {
        return string.IsNullOrWhiteSpace(sortBy)
            ? string.Empty
            : sortBy.Trim().Replace("_", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
    }
}
