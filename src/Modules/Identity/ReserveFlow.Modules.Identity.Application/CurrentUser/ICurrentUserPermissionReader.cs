namespace ReserveFlow.Modules.Identity.Application.CurrentUser;

public interface ICurrentUserPermissionReader
{
    Task<IReadOnlySet<string>> GetPermissionsAsync(
        Guid userId,
        Guid? tenantId,
        bool includePlatformPermissions,
        CancellationToken cancellationToken = default);
}
