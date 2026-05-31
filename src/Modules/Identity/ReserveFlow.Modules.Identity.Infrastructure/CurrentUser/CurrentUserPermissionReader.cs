using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Identity.Application.CurrentUser;
using ReserveFlow.Modules.Identity.Infrastructure.Database;

namespace ReserveFlow.Modules.Identity.Infrastructure.CurrentUser;

internal sealed class CurrentUserPermissionReader(IdentityDbContext dbContext) : ICurrentUserPermissionReader
{
    public async Task<IReadOnlySet<string>> GetPermissionsAsync(
        Guid userId,
        Guid? tenantId,
        bool includePlatformPermissions,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Guid> roleIds = dbContext.UserRoles
            .Where(userRole =>
                userRole.UserId == userId &&
                (userRole.TenantId == tenantId ||
                 includePlatformPermissions && userRole.TenantId == null))
            .Select(userRole => userRole.RoleId);

        string[] permissions = await dbContext.RolePermissions
            .Where(rolePermission => roleIds.Contains(rolePermission.RoleId))
            .Join(
                dbContext.Permissions,
                rolePermission => rolePermission.PermissionId,
                permission => permission.Id,
                (_, permission) => permission.Code)
            .Distinct()
            .OrderBy(code => code)
            .ToArrayAsync(cancellationToken);

        return permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
