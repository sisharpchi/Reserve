using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Identity.Application.Keycloak;
using ReserveFlow.Modules.Identity.Application.Users;
using ReserveFlow.Modules.Identity.Domain.TenantUsers;
using ReserveFlow.Modules.Identity.Domain.Users;

namespace ReserveFlow.Modules.Identity.Application.TenantUsers.InviteTenantOwner;

public sealed class InviteTenantOwnerCommandHandler(
    IKeycloakAdminClient keycloakAdminClient,
    IUserRepository userRepository,
    ITenantUserRepository tenantUserRepository,
    IIdentityUnitOfWork unitOfWork)
    : ICommandHandler<InviteTenantOwnerCommand, InviteTenantOwnerResponse>
{
    private const string TenantAdminRole = "TenantAdmin";

    public async Task<InviteTenantOwnerResponse> Handle(
        InviteTenantOwnerCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.TenantId == Guid.Empty)
        {
            throw new InvalidOperationException("Tenant id is required.");
        }

        string email = NormalizeRequired(command.Email, "Email").ToLowerInvariant();
        string displayName = NormalizeOptional(command.DisplayName) ?? email;

        KeycloakProvisionedUser provisionedUser = await keycloakAdminClient.ProvisionTenantAdminAsync(
            email,
            displayName,
            cancellationToken);

        string keycloakSubject = NormalizeRequired(provisionedUser.KeycloakSubject, "Keycloak subject");
        string provisionedEmail = NormalizeRequired(provisionedUser.Email, "Email").ToLowerInvariant();
        string provisionedDisplayName = NormalizeRequired(provisionedUser.DisplayName, "Display name");

        User? user = await userRepository.GetByKeycloakSubjectAsync(keycloakSubject, cancellationToken);
        bool localUserCreated = user is null;

        if (user is null)
        {
            user = User.Create(keycloakSubject, provisionedEmail, provisionedDisplayName);
            userRepository.Insert(user);
        }
        else
        {
            user.UpdateProfile(provisionedEmail, provisionedDisplayName);
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

        return new InviteTenantOwnerResponse(
            tenantUser.TenantId,
            user.Id,
            user.KeycloakSubject,
            user.Email,
            user.DisplayName,
            tenantUser.Role,
            provisionedUser.CreatedInKeycloak,
            provisionedUser.InvitationEmailSent,
            localUserCreated,
            membershipCreated);
    }

    private static string NormalizeRequired(string? value, string fieldName)
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
