using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Identity.Application.Users;
using ReserveFlow.Modules.Identity.Domain.TenantUsers;
using ReserveFlow.Modules.Identity.Domain.Users;

namespace ReserveFlow.Modules.Identity.Application.TenantUsers.AssignTenantOwner;

public sealed class AssignTenantOwnerCommandHandler(
    IUserRepository userRepository,
    ITenantUserRepository tenantUserRepository,
    IIdentityUnitOfWork unitOfWork)
    : ICommandHandler<AssignTenantOwnerCommand, AssignTenantOwnerResponse>
{
    private const string TenantAdminRole = "TenantAdmin";

    public async Task<AssignTenantOwnerResponse> Handle(
        AssignTenantOwnerCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.TenantId == Guid.Empty)
        {
            throw new InvalidOperationException("Tenant id is required.");
        }

        string keycloakSubject = NormalizeRequired(command.KeycloakSubject, "Keycloak subject");
        string email = NormalizeRequired(command.Email, "Email").ToLowerInvariant();
        string displayName = NormalizeOptional(command.DisplayName) ?? email;

        User? user = await userRepository.GetByKeycloakSubjectAsync(keycloakSubject, cancellationToken);
        bool userCreated = user is null;

        if (user is null)
        {
            user = User.Create(keycloakSubject, email, displayName);
            userRepository.Insert(user);
        }
        else
        {
            user.UpdateProfile(email, displayName);
        }

        TenantUser? tenantUser = await tenantUserRepository.GetByTenantAndUserIdAsync(
            command.TenantId,
            user.Id,
            cancellationToken);
        bool membershipCreated = tenantUser is null;

        if (tenantUser is null)
        {
            tenantUser = TenantUser.Create(command.TenantId, user.Id, TenantAdminRole);
            tenantUserRepository.Insert(tenantUser);
        }
        else
        {
            tenantUser.AssignRole(TenantAdminRole);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AssignTenantOwnerResponse(
            tenantUser.TenantId,
            user.Id,
            user.KeycloakSubject,
            user.Email,
            user.DisplayName,
            tenantUser.Role,
            userCreated,
            membershipCreated);
    }

    private static string NormalizeRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
