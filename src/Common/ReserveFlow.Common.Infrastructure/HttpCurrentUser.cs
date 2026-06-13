using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ReserveFlow.Common.Application.Abstractions;

namespace ReserveFlow.Common.Infrastructure;

public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private static readonly StringComparer PermissionComparer = StringComparer.OrdinalIgnoreCase;

    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            string? value =
                GetClaimValue("user_id") ??
                GetClaimValue("uid") ??
                GetClaimValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(value, out Guid userId))
            {
                return userId;
            }

            string? subject = KeycloakSubject;

            return Guid.TryParse(subject, out Guid subjectAsUserId)
                ? subjectAsUserId
                : null;
        }
    }

    public Guid? StaffMemberId
    {
        get
        {
            string? value = GetClaimValue("staff_member_id") ?? GetClaimValue("staff_id");

            return Guid.TryParse(value, out Guid staffMemberId)
                ? staffMemberId
                : null;
        }
    }

    public string? KeycloakSubject => NormalizeOptional(GetClaimValue("sub") ?? GetClaimValue(ClaimTypes.NameIdentifier));

    public string? Email => NormalizeOptional(GetClaimValue("email") ?? GetClaimValue(ClaimTypes.Email))?.ToLowerInvariant();

    public IReadOnlySet<string> Permissions
    {
        get
        {
            HashSet<string> permissions = new(PermissionComparer);

            foreach (Claim claim in Principal?.Claims ?? [])
            {
                if (!IsPermissionClaim(claim.Type))
                {
                    continue;
                }

                foreach (string permission in SplitClaimValues(claim.Value))
                {
                    permissions.Add(permission);
                }
            }

            return permissions;
        }
    }

    private string? GetClaimValue(string claimType)
    {
        return Principal?.FindFirst(claimType)?.Value;
    }

    private static bool IsPermissionClaim(string claimType)
    {
        return claimType is "permissions" or "permission" or "scope" or "scp";
    }

    private static IEnumerable<string> SplitClaimValues(string value)
    {
        return value
            .Split([' ', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => !string.IsNullOrWhiteSpace(item));
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
