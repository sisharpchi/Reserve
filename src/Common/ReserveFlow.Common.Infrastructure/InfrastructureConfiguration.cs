using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Data;
using ReserveFlow.Common.Application.RateLimiting;
using ReserveFlow.Common.Infrastructure.Data;
using ReserveFlow.Common.Infrastructure.Errors;
using ReserveFlow.Common.Infrastructure.Identity;
using ReserveFlow.Common.Infrastructure.Observability;

namespace ReserveFlow.Common.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddReserveFlowInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            };
        });
        services.AddCors(options =>
        {
            string[] allowedOrigins = configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? [];

            options.AddPolicy(CorsPolicies.WebApp, policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins);
                }
                else
                {
                    policy.AllowAnyOrigin();
                }

                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
            });
        });

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddScoped<ITenantContext, HttpTenantContext>();
        services.AddScoped<ITenantAccessGuard, TenantAccessGuard>();
        services.AddSingleton<IDatabaseConnectionStringProvider, DatabaseConnectionStringProvider>();
        services.Configure<DatabaseInitializerOptions>(configuration.GetSection("DatabaseInitializer"));
        services.AddHostedService<DatabaseSchemaInitializerHostedService>();
        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: GetOpenTelemetryServiceName(configuration)))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(ReserveFlowTelemetry.ActivitySourceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                AddOtlpExporterIfEnabled(tracing, configuration);
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(ReserveFlowTelemetry.MeterName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                AddOtlpExporterIfEnabled(metrics, configuration);
            });

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            AddFixedWindowPolicy(
                options,
                RateLimitPolicies.PublicBooking,
                permitLimit: configuration.GetValue("RateLimiting:PublicBooking:PermitLimit", 10),
                window: TimeSpan.FromMinutes(configuration.GetValue("RateLimiting:PublicBooking:WindowMinutes", 1)));

            AddFixedWindowPolicy(
                options,
                RateLimitPolicies.PublicAvailability,
                permitLimit: configuration.GetValue("RateLimiting:PublicAvailability:PermitLimit", 60),
                window: TimeSpan.FromMinutes(configuration.GetValue("RateLimiting:PublicAvailability:WindowMinutes", 1)));

            AddFixedWindowPolicy(
                options,
                RateLimitPolicies.AuthContext,
                permitLimit: configuration.GetValue("RateLimiting:AuthContext:PermitLimit", 30),
                window: TimeSpan.FromMinutes(configuration.GetValue("RateLimiting:AuthContext:WindowMinutes", 1)));
        });

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                string? authority = configuration["Identity:Authority"];
                string? metadataAddress = configuration["Identity:MetadataAddress"];
                string? audience = configuration["Identity:Audience"];

                if (!string.IsNullOrWhiteSpace(authority))
                {
                    options.Authority = authority;
                }

                if (!string.IsNullOrWhiteSpace(metadataAddress))
                {
                    options.MetadataAddress = metadataAddress;
                }

                if (!string.IsNullOrWhiteSpace(audience))
                {
                    options.Audience = audience;
                }

                options.RequireHttpsMetadata = configuration.GetValue("Identity:RequireHttpsMetadata", defaultValue: false);
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "preferred_username",
                    RoleClaimType = "roles",
                    ValidateAudience = !string.IsNullOrWhiteSpace(audience)
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(
                AuthorizationPolicies.PlatformAdmin,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context =>
                        KeycloakRoleClaims.HasAnyRole(context.User, KeycloakRoles.PlatformAdmin)))
            .AddPolicy(
                AuthorizationPolicies.TenantAdmin,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context =>
                        KeycloakRoleClaims.HasAnyRole(
                            context.User,
                            KeycloakRoles.PlatformAdmin,
                            KeycloakRoles.TenantAdmin)))
            .AddPolicy(
                AuthorizationPolicies.Staff,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context =>
                        KeycloakRoleClaims.HasAnyRole(
                            context.User,
                            KeycloakRoles.PlatformAdmin,
                            KeycloakRoles.TenantAdmin,
                            KeycloakRoles.Staff)));

        IHealthChecksBuilder healthChecks = services
            .AddHealthChecks()
            .AddCheck<PostgresHealthCheck>("postgresql", tags: ["ready"]);

        string? keycloakHealthUrl = configuration["Keycloak:HealthUrl"];

        if (Uri.TryCreate(keycloakHealthUrl, UriKind.Absolute, out Uri? keycloakHealthUri))
        {
            services.Configure<KeycloakHealthCheckOptions>(options => options.HealthUrl = keycloakHealthUri);
            services.AddHttpClient(KeycloakHealthCheck.HttpClientName);
            healthChecks.AddCheck<KeycloakHealthCheck>("keycloak", tags: ["ready"]);
        }

        return services;
    }

    private static void AddFixedWindowPolicy(
        RateLimiterOptions options,
        string policyName,
        int permitLimit,
        TimeSpan window)
    {
        options.AddPolicy(
            policyName,
            httpContext => RateLimitPartition.GetFixedWindowLimiter(
                GetRateLimitPartitionKey(httpContext),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = window,
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                }));
    }

    private static string GetRateLimitPartitionKey(HttpContext httpContext)
    {
        return httpContext.User.Identity?.IsAuthenticated == true
            ? $"user:{httpContext.User.Identity.Name}"
            : $"ip:{httpContext.Connection.RemoteIpAddress}";
    }

    private static string GetOpenTelemetryServiceName(IConfiguration configuration)
    {
        string? serviceName = configuration["OpenTelemetry:ServiceName"];

        return string.IsNullOrWhiteSpace(serviceName)
            ? ReserveFlowTelemetry.DefaultServiceName
            : serviceName;
    }

    private static void AddOtlpExporterIfEnabled(
        TracerProviderBuilder tracing,
        IConfiguration configuration)
    {
        if (!configuration.GetValue("OpenTelemetry:Otlp:Enabled", defaultValue: false))
        {
            return;
        }

        tracing.AddOtlpExporter(options =>
        {
            string? endpoint = configuration["OpenTelemetry:Otlp:Endpoint"];

            if (Uri.TryCreate(endpoint, UriKind.Absolute, out Uri? uri))
            {
                options.Endpoint = uri;
            }
        });
    }

    private static void AddOtlpExporterIfEnabled(
        MeterProviderBuilder metrics,
        IConfiguration configuration)
    {
        if (!configuration.GetValue("OpenTelemetry:Otlp:Enabled", defaultValue: false))
        {
            return;
        }

        metrics.AddOtlpExporter(options =>
        {
            string? endpoint = configuration["OpenTelemetry:Otlp:Endpoint"];

            if (Uri.TryCreate(endpoint, UriKind.Absolute, out Uri? uri))
            {
                options.Endpoint = uri;
            }
        });
    }
}
