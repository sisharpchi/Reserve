using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Identity.Application.Users;
using ReserveFlow.Modules.Identity.Domain.Users;

namespace ReserveFlow.Modules.Identity.Application.CurrentUser;

public sealed class GetCurrentUserQueryHandler(
    ICurrentUser currentUser,
    ITenantContext tenantContext,
    IUserRepository userRepository,
    ICurrentUserPermissionReader permissionReader)
    : IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    private static readonly StringComparer PermissionComparer = StringComparer.OrdinalIgnoreCase;

    public async Task<CurrentUserResponse> Handle(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken = default)
    {
        string? keycloakSubject = currentUser.KeycloakSubject;
        User? localUser = string.IsNullOrWhiteSpace(keycloakSubject)
            ? null
            : await userRepository.GetByKeycloakSubjectAsync(keycloakSubject, cancellationToken);

        Guid? userId = localUser?.Id ?? currentUser.UserId;
        HashSet<string> permissions = new(currentUser.Permissions, PermissionComparer);

        if (localUser is not null)
        {
            IReadOnlySet<string> databasePermissions = await permissionReader.GetPermissionsAsync(
                localUser.Id,
                tenantContext.TenantId,
                tenantContext.IsPlatformScope,
                cancellationToken);

            permissions.UnionWith(databasePermissions);
        }

        return new CurrentUserResponse(
            IsAuthenticated: keycloakSubject is not null,
            UserId: userId,
            KeycloakSubject: keycloakSubject,
            Email: localUser?.Email ?? currentUser.Email,
            Permissions: permissions.Order(PermissionComparer).ToArray(),
            Tenant: new CurrentTenantResponse(
                tenantContext.TenantId,
                tenantContext.TenantSlug,
                tenantContext.TimeZoneId,
                tenantContext.IsPlatformScope));
    }
}
