using System.Security.Claims;
using System.Text.Json;

namespace ReserveFlow.Common.Infrastructure;

public static class KeycloakRoleClaims
{
    private static readonly StringComparer RoleComparer = StringComparer.OrdinalIgnoreCase;

    public static bool HasAnyRole(ClaimsPrincipal principal, params string[] roles)
    {
        if (roles.Length == 0)
        {
            return false;
        }

        HashSet<string> expectedRoles = roles.ToHashSet(RoleComparer);

        return GetRoles(principal).Any(role => MatchesAnyRole(role, expectedRoles));
    }

    private static IEnumerable<string> GetRoles(ClaimsPrincipal principal)
    {
        foreach (Claim claim in principal.Claims)
        {
            if (IsSimpleRoleClaim(claim.Type))
            {
                foreach (string role in SplitRoleClaim(claim.Value))
                {
                    yield return role;
                }
            }
            else if (claim.Type == "realm_access")
            {
                foreach (string role in ReadRealmAccessRoles(claim.Value))
                {
                    yield return role;
                }
            }
            else if (claim.Type == "resource_access")
            {
                foreach (string role in ReadResourceAccessRoles(claim.Value))
                {
                    yield return role;
                }
            }
        }
    }

    private static bool IsSimpleRoleClaim(string claimType)
    {
        return claimType is "roles" or "role" || claimType == ClaimTypes.Role;
    }

    private static IEnumerable<string> SplitRoleClaim(string value)
    {
        return value
            .Split([' ', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(role => !string.IsNullOrWhiteSpace(role));
    }

    private static IEnumerable<string> ReadRealmAccessRoles(string value)
    {
        using JsonDocument? document = TryParseJson(value);

        if (document is null ||
            !document.RootElement.TryGetProperty("roles", out JsonElement rolesElement))
        {
            yield break;
        }

        foreach (string role in ReadStringArray(rolesElement))
        {
            yield return role;
        }
    }

    private static IEnumerable<string> ReadResourceAccessRoles(string value)
    {
        using JsonDocument? document = TryParseJson(value);

        if (document is null || document.RootElement.ValueKind != JsonValueKind.Object)
        {
            yield break;
        }

        foreach (JsonProperty clientAccess in document.RootElement.EnumerateObject())
        {
            if (!clientAccess.Value.TryGetProperty("roles", out JsonElement rolesElement))
            {
                continue;
            }

            foreach (string role in ReadStringArray(rolesElement))
            {
                yield return role;
            }
        }
    }

    private static IEnumerable<string> ReadStringArray(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (JsonElement item in element.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                string? role = item.GetString();

                if (!string.IsNullOrWhiteSpace(role))
                {
                    yield return role;
                }
            }
        }
    }

    private static JsonDocument? TryParseJson(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            return JsonDocument.Parse(value);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static bool MatchesAnyRole(string actualRole, HashSet<string> expectedRoles)
    {
        if (expectedRoles.Contains(actualRole))
        {
            return true;
        }

        string normalizedActualRole = NormalizeRoleName(actualRole);

        return expectedRoles.Any(role => NormalizeRoleName(role) == normalizedActualRole);
    }

    private static string NormalizeRoleName(string role)
    {
        return role
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();
    }
}
