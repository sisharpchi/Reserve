using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ReserveFlow.Common.Application.Data;
using ReserveFlow.Common.Application.RateLimiting;
using ReserveFlow.Common.Infrastructure.Data;

namespace ReserveFlow.Common.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddReserveFlowInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddProblemDetails();
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

        services.AddSingleton<IDatabaseConnectionStringProvider, DatabaseConnectionStringProvider>();
        services.Configure<DatabaseInitializerOptions>(configuration.GetSection("DatabaseInitializer"));
        services.AddHostedService<DatabaseSchemaInitializerHostedService>();
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

        services
            .AddHealthChecks()
            .AddCheck<PostgresHealthCheck>("postgresql", tags: ["ready"]);

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
}
