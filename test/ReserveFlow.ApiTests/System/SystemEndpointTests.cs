using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ReserveFlow.ApiTests.System;

public sealed class SystemEndpointTests(ReserveFlowApiFactory factory)
    : IClassFixture<ReserveFlowApiFactory>
{
    [Fact]
    public async Task RootEndpointReturnsSystemMetadata()
    {
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/");

        response.EnsureSuccessStatusCode();

        await using Stream stream = await response.Content.ReadAsStreamAsync();
        using JsonDocument document = await JsonDocument.ParseAsync(stream);

        JsonElement root = document.RootElement;

        Assert.Equal("ReserveFlow", root.GetProperty("application").GetString());
        Assert.Equal(".NET 8", root.GetProperty("runtime").GetString());
        Assert.Equal("Modular Monolith", root.GetProperty("architecture").GetString());
        Assert.Equal("Keycloak", root.GetProperty("identityProvider").GetString());
    }

    [Fact]
    public async Task LiveHealthEndpointReturnsHealthy()
    {
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/health/live");

        response.EnsureSuccessStatusCode();

        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal("Healthy", body);
    }

    [Fact]
    public async Task RootEndpointReturnsSecurityHeaders()
    {
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/");

        response.EnsureSuccessStatusCode();

        AssertHeader(response, "X-Content-Type-Options", "nosniff");
        AssertHeader(response, "X-Frame-Options", "DENY");
        AssertHeader(response, "Referrer-Policy", "no-referrer");
        AssertHeader(response, "Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
        AssertHeader(response, "Content-Security-Policy", "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'");
    }

    [Fact]
    public async Task ProtectedAuthEndpointReturnsUnauthorizedWithoutToken()
    {
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static void AssertHeader(HttpResponseMessage response, string name, string expectedValue)
    {
        Assert.True(response.Headers.TryGetValues(name, out IEnumerable<string>? values), $"Missing header {name}.");
        Assert.Contains(expectedValue, values);
    }
}

public sealed class ReserveFlowApiFactory : WebApplicationFactory<Program>
{
    private static readonly string[] HostedServiceTypeNames =
    [
        "ReserveFlow.Common.Infrastructure.Data.DatabaseSchemaInitializerHostedService",
        "ReserveFlow.Modules.Bookings.Infrastructure.Outbox.BookingsOutboxProcessorHostedService",
        "ReserveFlow.Modules.Notifications.Infrastructure.Delivery.NotificationDeliveryHostedService"
    ];

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            Dictionary<string, string?> configuration = new()
            {
                ["ConnectionStrings:Database"] = "Host=localhost;Port=1;Database=reserveflow_api_tests;Username=postgres;Password=postgres",
                ["DatabaseInitializer:Enabled"] = "false",
                ["Tenants:SeedDemoTenant"] = "false",
                ["Keycloak:HealthUrl"] = string.Empty,
                ["OpenTelemetry:Otlp:Enabled"] = "false"
            };

            configurationBuilder.AddInMemoryCollection(configuration);
        });

        builder.ConfigureServices(services =>
        {
            ServiceDescriptor[] hostedServices = services
                .Where(static service => service.ServiceType == typeof(IHostedService))
                .Where(static service => service.ImplementationType is not null &&
                    HostedServiceTypeNames.Contains(service.ImplementationType.FullName, StringComparer.Ordinal))
                .ToArray();

            foreach (ServiceDescriptor hostedService in hostedServices)
            {
                services.Remove(hostedService);
            }
        });
    }
}
