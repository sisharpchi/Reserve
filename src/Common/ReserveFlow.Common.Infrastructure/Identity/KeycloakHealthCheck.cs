using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace ReserveFlow.Common.Infrastructure.Identity;

internal sealed class KeycloakHealthCheck(
    IHttpClientFactory httpClientFactory,
    IOptions<KeycloakHealthCheckOptions> options) : IHealthCheck
{
    internal const string HttpClientName = "keycloak-health";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (options.Value.HealthUrl is null)
        {
            return HealthCheckResult.Unhealthy("Keycloak health URL is not configured.");
        }

        try
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientName);
            using HttpResponseMessage response = await httpClient.GetAsync(options.Value.HealthUrl, cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("Keycloak is reachable.")
                : HealthCheckResult.Unhealthy($"Keycloak returned HTTP {(int)response.StatusCode}.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Keycloak is not reachable.", exception);
        }
    }
}
