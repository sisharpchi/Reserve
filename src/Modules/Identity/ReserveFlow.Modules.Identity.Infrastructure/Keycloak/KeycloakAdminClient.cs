using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using ReserveFlow.Modules.Identity.Application.Keycloak;

namespace ReserveFlow.Modules.Identity.Infrastructure.Keycloak;

internal sealed class KeycloakAdminClient(
    HttpClient httpClient,
    IOptions<KeycloakAdminOptions> options)
    : IKeycloakAdminClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string AdminRealmsPath = "/admin/realms/";
    private const string UsersPath = "/users";
    private const string RealmRoleMappingsPath = "/role-mappings/realm";

    public async Task<KeycloakProvisionedUser> ProvisionTenantAdminAsync(
        string email,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        KeycloakAdminOptions keycloakOptions = ValidateOptions(options.Value);
        string normalizedEmail = NormalizeRequired(email, "Email").ToLowerInvariant();
        string normalizedDisplayName = NormalizeOptional(displayName) ?? normalizedEmail;
        string accessToken = await RequestAccessTokenAsync(keycloakOptions, cancellationToken);

        KeycloakUserRepresentation? user = await FindUserByEmailAsync(
            keycloakOptions,
            accessToken,
            normalizedEmail,
            cancellationToken);
        bool created = user is null;

        if (user is null)
        {
            string userId = await CreateUserAsync(
                keycloakOptions,
                accessToken,
                normalizedEmail,
                normalizedDisplayName,
                cancellationToken);

            user = await FindUserByIdAsync(keycloakOptions, accessToken, userId, cancellationToken);
        }

        string userIdValue = NormalizeRequired(user.Id, "Keycloak user id");
        await AssignRealmRoleAsync(
            keycloakOptions,
            accessToken,
            userIdValue,
            keycloakOptions.TenantAdminRealmRole,
            cancellationToken);

        bool invitationEmailSent = false;
        if (keycloakOptions.SendInvitationEmail)
        {
            await SendRequiredActionsEmailAsync(
                keycloakOptions,
                accessToken,
                userIdValue,
                cancellationToken);
            invitationEmailSent = true;
        }

        return new KeycloakProvisionedUser(
            userIdValue,
            NormalizeOptional(user.Email) ?? normalizedEmail,
            NormalizeOptional(user.FirstName) ?? normalizedDisplayName,
            created,
            invitationEmailSent);
    }

    private async Task<string> RequestAccessTokenAsync(
        KeycloakAdminOptions keycloakOptions,
        CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(
            HttpMethod.Post,
            $"{BaseUrl(keycloakOptions)}/realms/{Uri.EscapeDataString(keycloakOptions.Realm)}/protocol/openid-connect/token");
        request.Content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = keycloakOptions.ClientId,
                ["client_secret"] = keycloakOptions.ClientSecret
            });

        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "request Keycloak admin access token", cancellationToken);

        KeycloakTokenResponse? token = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(
            JsonOptions,
            cancellationToken);

        return NormalizeRequired(token?.AccessToken, "Keycloak access token");
    }

    private async Task<KeycloakUserRepresentation?> FindUserByEmailAsync(
        KeycloakAdminOptions keycloakOptions,
        string accessToken,
        string email,
        CancellationToken cancellationToken)
    {
        string requestUri =
            $"{AdminBasePath(keycloakOptions)}{UsersPath}?email={Uri.EscapeDataString(email)}&exact=true&max=2";
        using HttpResponseMessage response = await SendAuthenticatedAsync(
            HttpMethod.Get,
            requestUri,
            accessToken,
            content: null,
            cancellationToken);
        await EnsureSuccessAsync(response, "search Keycloak user by email", cancellationToken);

        List<KeycloakUserRepresentation>? users = await response.Content.ReadFromJsonAsync<List<KeycloakUserRepresentation>>(
            JsonOptions,
            cancellationToken);

        return users?.FirstOrDefault(user => string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<KeycloakUserRepresentation> FindUserByIdAsync(
        KeycloakAdminOptions keycloakOptions,
        string accessToken,
        string userId,
        CancellationToken cancellationToken)
    {
        string requestUri = $"{AdminBasePath(keycloakOptions)}{UsersPath}/{Uri.EscapeDataString(userId)}";
        using HttpResponseMessage response = await SendAuthenticatedAsync(
            HttpMethod.Get,
            requestUri,
            accessToken,
            content: null,
            cancellationToken);
        await EnsureSuccessAsync(response, "read created Keycloak user", cancellationToken);

        KeycloakUserRepresentation? user = await response.Content.ReadFromJsonAsync<KeycloakUserRepresentation>(
            JsonOptions,
            cancellationToken);

        return user ?? throw new InvalidOperationException("Keycloak returned an empty user response.");
    }

    private async Task<string> CreateUserAsync(
        KeycloakAdminOptions keycloakOptions,
        string accessToken,
        string email,
        string displayName,
        CancellationToken cancellationToken)
    {
        var payload = new KeycloakUserRepresentation
        {
            Username = email,
            Email = email,
            FirstName = displayName,
            Enabled = true,
            EmailVerified = false,
            RequiredActions = keycloakOptions.RequiredActions
        };
        using JsonContent content = JsonContent.Create(payload, options: JsonOptions);
        using HttpResponseMessage response = await SendAuthenticatedAsync(
            HttpMethod.Post,
            $"{AdminBasePath(keycloakOptions)}{UsersPath}",
            accessToken,
            content,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            KeycloakUserRepresentation? existing = await FindUserByEmailAsync(
                keycloakOptions,
                accessToken,
                email,
                cancellationToken);

            return NormalizeRequired(existing?.Id, "Existing Keycloak user id");
        }

        await EnsureSuccessAsync(response, "create Keycloak user", cancellationToken);
        string? userId = TryReadUserIdFromLocation(response.Headers.Location);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            return userId;
        }

        KeycloakUserRepresentation? createdUser = await FindUserByEmailAsync(
            keycloakOptions,
            accessToken,
            email,
            cancellationToken);

        return NormalizeRequired(createdUser?.Id, "Created Keycloak user id");
    }

    private async Task AssignRealmRoleAsync(
        KeycloakAdminOptions keycloakOptions,
        string accessToken,
        string userId,
        string realmRole,
        CancellationToken cancellationToken)
    {
        string normalizedRole = NormalizeRequired(realmRole, "Tenant admin realm role");
        string roleUri = $"{AdminBasePath(keycloakOptions)}/roles/{Uri.EscapeDataString(normalizedRole)}";
        using HttpResponseMessage roleResponse = await SendAuthenticatedAsync(
            HttpMethod.Get,
            roleUri,
            accessToken,
            content: null,
            cancellationToken);
        await EnsureSuccessAsync(roleResponse, "read Keycloak tenant admin realm role", cancellationToken);

        KeycloakRoleRepresentation? role = await roleResponse.Content.ReadFromJsonAsync<KeycloakRoleRepresentation>(
            JsonOptions,
            cancellationToken);

        if (role is null || string.IsNullOrWhiteSpace(role.Name))
        {
            throw new InvalidOperationException($"Keycloak realm role '{normalizedRole}' was not returned.");
        }

        using JsonContent content = JsonContent.Create(new[] { role }, options: JsonOptions);
        using HttpResponseMessage response = await SendAuthenticatedAsync(
            HttpMethod.Post,
            $"{AdminBasePath(keycloakOptions)}{UsersPath}/{Uri.EscapeDataString(userId)}{RealmRoleMappingsPath}",
            accessToken,
            content,
            cancellationToken);
        await EnsureSuccessAsync(response, "map Keycloak tenant admin realm role", cancellationToken);
    }

    private async Task SendRequiredActionsEmailAsync(
        KeycloakAdminOptions keycloakOptions,
        string accessToken,
        string userId,
        CancellationToken cancellationToken)
    {
        string requestUri =
            $"{AdminBasePath(keycloakOptions)}{UsersPath}/{Uri.EscapeDataString(userId)}/execute-actions-email";
        List<string> queryParts = [];

        if (!string.IsNullOrWhiteSpace(keycloakOptions.InvitationClientId))
        {
            queryParts.Add($"client_id={Uri.EscapeDataString(keycloakOptions.InvitationClientId)}");
        }

        if (!string.IsNullOrWhiteSpace(keycloakOptions.InvitationRedirectUri))
        {
            queryParts.Add($"redirect_uri={Uri.EscapeDataString(keycloakOptions.InvitationRedirectUri)}");
        }

        if (keycloakOptions.InvitationLifespanSeconds > 0)
        {
            queryParts.Add($"lifespan={keycloakOptions.InvitationLifespanSeconds}");
        }

        if (queryParts.Count > 0)
        {
            requestUri += "?" + string.Join("&", queryParts);
        }

        string[] requiredActions = keycloakOptions.RequiredActions.Length == 0
            ? ["UPDATE_PASSWORD"]
            : keycloakOptions.RequiredActions;
        using JsonContent content = JsonContent.Create(requiredActions, options: JsonOptions);
        using HttpResponseMessage response = await SendAuthenticatedAsync(
            HttpMethod.Put,
            requestUri,
            accessToken,
            content,
            cancellationToken);
        await EnsureSuccessAsync(response, "send Keycloak required-actions invitation email", cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAuthenticatedAsync(
        HttpMethod method,
        string requestUri,
        string accessToken,
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(method, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = content;

        return await httpClient.SendAsync(request, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string operation,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(
            $"Unable to {operation}. Keycloak returned HTTP {(int)response.StatusCode}: {body}");
    }

    private static KeycloakAdminOptions ValidateOptions(KeycloakAdminOptions keycloakOptions)
    {
        _ = NormalizeRequired(keycloakOptions.BaseUrl, "Keycloak admin base URL");
        _ = NormalizeRequired(keycloakOptions.Realm, "Keycloak realm");
        _ = NormalizeRequired(keycloakOptions.ClientId, "Keycloak admin client id");
        _ = NormalizeRequired(keycloakOptions.ClientSecret, "Keycloak admin client secret");
        _ = NormalizeRequired(keycloakOptions.TenantAdminRealmRole, "Keycloak tenant admin realm role");
        keycloakOptions.TenantAdminRealmRole = keycloakOptions.TenantAdminRealmRole.Trim();

        if (keycloakOptions.RequiredActions.Length == 0)
        {
            keycloakOptions.RequiredActions = ["UPDATE_PASSWORD"];
        }

        return keycloakOptions;
    }

    private static string BaseUrl(KeycloakAdminOptions keycloakOptions)
    {
        return NormalizeRequired(keycloakOptions.BaseUrl, "Keycloak admin base URL").TrimEnd('/');
    }

    private static string AdminBasePath(KeycloakAdminOptions keycloakOptions)
    {
        return $"{BaseUrl(keycloakOptions)}{AdminRealmsPath}{Uri.EscapeDataString(keycloakOptions.Realm)}";
    }

    private static string? TryReadUserIdFromLocation(Uri? location)
    {
        if (location is null)
        {
            return null;
        }

        return location.Segments.LastOrDefault()?.TrimEnd('/');
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

    private sealed record KeycloakTokenResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken);

    private sealed class KeycloakUserRepresentation
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("username")]
        public string? Username { get; init; }

        [JsonPropertyName("email")]
        public string? Email { get; init; }

        [JsonPropertyName("firstName")]
        public string? FirstName { get; init; }

        [JsonPropertyName("enabled")]
        public bool Enabled { get; init; }

        [JsonPropertyName("emailVerified")]
        public bool EmailVerified { get; init; }

        [JsonPropertyName("requiredActions")]
        public string[] RequiredActions { get; init; } = [];
    }

    private sealed class KeycloakRoleRepresentation
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("composite")]
        public bool Composite { get; init; }

        [JsonPropertyName("clientRole")]
        public bool ClientRole { get; init; }

        [JsonPropertyName("containerId")]
        public string? ContainerId { get; init; }
    }
}
