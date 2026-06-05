using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Domain;
using ReserveFlow.Common.Infrastructure;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Common.Infrastructure.Outbox;
using ReserveFlow.Modules.Audit.Application.AuditLogs;
using ReserveFlow.Modules.Audit.Application.AuditLogs.RecordAuditLog;
using ReserveFlow.Modules.Audit.Domain.AuditLogs;
using ReserveFlow.Modules.Audit.Infrastructure.Database;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.CancelBooking;
using ReserveFlow.Modules.Bookings.Application.Bookings.CompleteBooking;
using ReserveFlow.Modules.Bookings.Application.Bookings.CreateBooking;
using ReserveFlow.Modules.Bookings.Application.Bookings.MarkBookingAsNoShow;
using ReserveFlow.Modules.Bookings.Application.Bookings.RescheduleBooking;
using ReserveFlow.Modules.Bookings.Domain.Bookings;
using ReserveFlow.Modules.Bookings.Infrastructure.Database;
using ReserveFlow.Modules.Catalog.Application.Services;
using ReserveFlow.Modules.Catalog.Domain.Services;
using ReserveFlow.Modules.Catalog.Infrastructure.Database;
using ReserveFlow.Modules.Identity.Infrastructure.Database;
using ReserveFlow.Modules.Identity.Domain.Users;
using ReserveFlow.Modules.Integrations.Application.WebhookInbox;
using ReserveFlow.Modules.Integrations.Application.WebhookInbox.AcceptWebhook;
using ReserveFlow.Modules.Integrations.Domain.WebhookInbox;
using ReserveFlow.Modules.Integrations.Infrastructure.Database;
using ReserveFlow.Modules.Notifications.Application.Notifications;
using ReserveFlow.Modules.Notifications.Application.Notifications.QueueNotification;
using ReserveFlow.Modules.Notifications.Domain.Notifications;
using ReserveFlow.Modules.Notifications.Infrastructure.Database;
using ReserveFlow.Modules.Notifications.Infrastructure.Sending;
using ReserveFlow.Modules.Reporting.Application.Reports;
using ReserveFlow.Modules.Reporting.Application.Reports.GetDailyBookingReport;
using ReserveFlow.Modules.Reporting.Application.Reports.RecordDailyBookingReport;
using ReserveFlow.Modules.Reporting.Domain.Reports;
using ReserveFlow.Modules.Reporting.Infrastructure.Database;
using ReserveFlow.Modules.Resources.Application.Resources;
using ReserveFlow.Modules.Resources.Domain.Resources;
using ReserveFlow.Modules.Resources.Infrastructure.Database;
using ReserveFlow.Modules.Scheduling.Application.Availability;
using ReserveFlow.Modules.Scheduling.Application.Availability.GetTenantAvailableSlots;
using ReserveFlow.Modules.Scheduling.Application.WorkingHours;
using ReserveFlow.Modules.Scheduling.Domain.Availability;
using ReserveFlow.Modules.Scheduling.Domain.UnavailablePeriods;
using ReserveFlow.Modules.Scheduling.Domain.WorkingHours;
using ReserveFlow.Modules.Scheduling.Infrastructure.Database;
using ReserveFlow.Modules.Staffing.Application.StaffMembers;
using ReserveFlow.Modules.Staffing.Domain.StaffMembers;
using ReserveFlow.Modules.Staffing.Infrastructure.Database;
using ReserveFlow.Modules.Tenants.Application.TenantCategories;
using ReserveFlow.Modules.Tenants.Application.Tenants;
using ReserveFlow.Modules.Tenants.Domain.Tenants;
using ReserveFlow.Modules.Tenants.Domain.TenantCategories;
using ReserveFlow.Modules.Tenants.Infrastructure.Database;

var checks = new List<(string Name, bool Passed)>
{
    ("identity presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Identity.Presentation.AssemblyReference.Assembly)),
    ("tenants presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Tenants.Presentation.AssemblyReference.Assembly)),
    ("catalog presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Catalog.Presentation.AssemblyReference.Assembly)),
    ("staffing presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Staffing.Presentation.AssemblyReference.Assembly)),
    ("resources presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Resources.Presentation.AssemblyReference.Assembly)),
    ("scheduling presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Scheduling.Presentation.AssemblyReference.Assembly)),
    ("bookings presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly)),
    ("notifications presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Notifications.Presentation.AssemblyReference.Assembly)),
    ("audit presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Audit.Presentation.AssemblyReference.Assembly)),
    ("reporting presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Reporting.Presentation.AssemblyReference.Assembly)),
    ("integrations presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Integrations.Presentation.AssemblyReference.Assembly)),
    ("common application defines rate limit policies", HasTypeNamed(
        typeof(ReserveFlow.Common.Application.Messaging.ICommand).Assembly,
        "RateLimitPolicies")),
    ("common infrastructure configures rate limiter", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/InfrastructureConfiguration.cs",
        "AddRateLimiter")),
    ("api pipeline uses rate limiter", SourceContains(
        "src/API/ReserveFlow.Api/Program.cs",
        "UseRateLimiter")),
    ("backend ci workflow exists", SourceContains(
        ".github/workflows/backend-ci.yml",
        "name: Backend CI")),
    ("backend ci installs dotnet 8 sdk", SourceContains(
        ".github/workflows/backend-ci.yml",
        "dotnet-version: 8.0.x")),
    ("backend ci restores solution", SourceContains(
        ".github/workflows/backend-ci.yml",
        "dotnet restore ReserveFlow.sln")),
    ("backend ci builds solution without restore", SourceContains(
        ".github/workflows/backend-ci.yml",
        "dotnet build ReserveFlow.sln --no-restore")),
    ("backend ci runs smoke tests", SourceContains(
        ".github/workflows/backend-ci.yml",
        "ReserveFlow.Backend.SmokeTests.csproj")),
    ("common infrastructure has correlation id middleware", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/Observability/CorrelationIdMiddleware.cs",
        "class CorrelationIdMiddleware")),
    ("correlation id middleware uses x-correlation-id header", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/Observability/CorrelationIdMiddleware.cs",
        "X-Correlation-Id")),
    ("correlation id middleware writes response header", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/Observability/CorrelationIdMiddleware.cs",
        "Response.Headers")),
    ("correlation id middleware adds logging scope", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/Observability/CorrelationIdMiddleware.cs",
        "BeginScope")),
    ("api pipeline uses correlation id middleware", SourceContains(
        "src/API/ReserveFlow.Api/Program.cs",
        "UseCorrelationId")),
    ("central packages pin serilog aspnetcore version", SourceContains(
        "Directory.Packages.props",
        "Serilog.AspNetCore")),
    ("api references serilog aspnetcore package", SourceContains(
        "src/API/ReserveFlow.Api/ReserveFlow.Api.csproj",
        "Serilog.AspNetCore")),
    ("api host uses serilog", SourceContains(
        "src/API/ReserveFlow.Api/Program.cs",
        "UseSerilog")),
    ("api host enriches serilog from log context", SourceContains(
        "src/API/ReserveFlow.Api/Program.cs",
        "Enrich.FromLogContext")),
    ("api config has serilog console sink", SourceContains(
        "src/API/ReserveFlow.Api/appsettings.json",
        "\"Name\": \"Console\"")),
    ("common infrastructure has global exception handler", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/Errors/GlobalExceptionHandler.cs",
        "class GlobalExceptionHandler")),
    ("global exception handler writes problem details", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/Errors/GlobalExceptionHandler.cs",
        "IProblemDetailsService")),
    ("global exception handler includes trace id", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/Errors/GlobalExceptionHandler.cs",
        "traceId")),
    ("common infrastructure registers exception handler", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/InfrastructureConfiguration.cs",
        "AddExceptionHandler<GlobalExceptionHandler>")),
    ("central packages pin opentelemetry hosting version", SourceContains(
        "Directory.Packages.props",
        "OpenTelemetry.Extensions.Hosting")),
    ("central packages pin opentelemetry aspnetcore instrumentation version", SourceContains(
        "Directory.Packages.props",
        "OpenTelemetry.Instrumentation.AspNetCore")),
    ("central packages pin opentelemetry http instrumentation version", SourceContains(
        "Directory.Packages.props",
        "OpenTelemetry.Instrumentation.Http")),
    ("central packages pin opentelemetry runtime instrumentation version", SourceContains(
        "Directory.Packages.props",
        "OpenTelemetry.Instrumentation.Runtime")),
    ("central packages pin opentelemetry otlp exporter version", SourceContains(
        "Directory.Packages.props",
        "OpenTelemetry.Exporter.OpenTelemetryProtocol")),
    ("common infrastructure references opentelemetry hosting package", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/ReserveFlow.Common.Infrastructure.csproj",
        "OpenTelemetry.Extensions.Hosting")),
    ("common infrastructure defines telemetry names", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/Observability/ReserveFlowTelemetry.cs",
        "class ReserveFlowTelemetry")),
    ("common infrastructure registers opentelemetry", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/InfrastructureConfiguration.cs",
        "AddOpenTelemetry")),
    ("common infrastructure configures opentelemetry tracing", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/InfrastructureConfiguration.cs",
        "WithTracing")),
    ("common infrastructure configures opentelemetry metrics", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/InfrastructureConfiguration.cs",
        "WithMetrics")),
    ("common infrastructure adds aspnetcore instrumentation", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/InfrastructureConfiguration.cs",
        "AddAspNetCoreInstrumentation")),
    ("common infrastructure adds http client instrumentation", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/InfrastructureConfiguration.cs",
        "AddHttpClientInstrumentation")),
    ("common infrastructure adds runtime instrumentation", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/InfrastructureConfiguration.cs",
        "AddRuntimeInstrumentation")),
    ("api config has opentelemetry service name", SourceContains(
        "src/API/ReserveFlow.Api/appsettings.json",
        "\"ServiceName\": \"ReserveFlow.Api\"")),
    ("api references swashbuckle openapi package", SourceContains(
        "src/API/ReserveFlow.Api/ReserveFlow.Api.csproj",
        "Swashbuckle.AspNetCore")),
    ("central packages pin swashbuckle openapi version", SourceContains(
        "Directory.Packages.props",
        "Swashbuckle.AspNetCore")),
    ("api services register swagger generator", SourceContains(
        "src/API/ReserveFlow.Api/Program.cs",
        "AddSwaggerGen")),
    ("api maps swagger json in development", SourceContains(
        "src/API/ReserveFlow.Api/Program.cs",
        "UseSwagger")),
    ("api maps swagger ui in development", SourceContains(
        "src/API/ReserveFlow.Api/Program.cs",
        "UseSwaggerUI")),
    ("public booking create endpoint is rate limited", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/CreateBookingEndpoint.cs",
        "RequireRateLimiting(RateLimitPolicies.PublicBooking")),
    ("public availability endpoint is rate limited", SourceContains(
        "src/Modules/Scheduling/ReserveFlow.Modules.Scheduling.Presentation/GetAvailableSlotsEndpoint.cs",
        "RequireRateLimiting(RateLimitPolicies.PublicAvailability")),
    ("auth me endpoint is rate limited", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Presentation/CurrentUserEndpoint.cs",
        "RequireRateLimiting(RateLimitPolicies.AuthContext")),
    ("auth me endpoint uses current user query handler", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Presentation/CurrentUserEndpoint.cs",
        "IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>")),
    ("auth sync-user endpoint exists", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Presentation/SyncUserEndpoint.cs",
        "\"/api/auth/sync-user\"")),
    ("auth sync-user endpoint is protected", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Presentation/SyncUserEndpoint.cs",
        "RequireAuthorization()")),
    ("auth sync-user endpoint is rate limited", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Presentation/SyncUserEndpoint.cs",
        "RequireRateLimiting(RateLimitPolicies.AuthContext)")),
    ("auth sync-user endpoint uses sync command handler", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Presentation/SyncUserEndpoint.cs",
        "ICommandHandler<SyncUserCommand, SyncUserResponse>")),
    ("platform assign tenant owner endpoint exists", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Presentation/AssignTenantOwnerEndpoint.cs",
        "\"/api/platform/tenants/{tenantId:guid}/assign-owner\"")),
    ("platform assign tenant owner endpoint requires platform admin", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Presentation/AssignTenantOwnerEndpoint.cs",
        "RequireAuthorization(PlatformAdminPolicy)")),
    ("platform assign tenant owner endpoint uses command handler", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Presentation/AssignTenantOwnerEndpoint.cs",
        "ICommandHandler<AssignTenantOwnerCommand, AssignTenantOwnerResponse>")),
    ("platform assign tenant owner request captures keycloak subject", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Presentation/AssignTenantOwnerRequest.cs",
        "KeycloakSubject")),
    ("common infrastructure has keycloak health check", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/Identity/KeycloakHealthCheck.cs",
        "class KeycloakHealthCheck")),
    ("common infrastructure registers keycloak readiness check", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/InfrastructureConfiguration.cs",
        "AddCheck<KeycloakHealthCheck>(\"keycloak\"")),
    ("keycloak readiness check is conditional on configured health url", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/InfrastructureConfiguration.cs",
        "Keycloak:HealthUrl")),
    ("authorization policies require keycloak role assertions", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/InfrastructureConfiguration.cs",
        "KeycloakRoleClaims.HasAnyRole")),
    ("keycloak role parser accepts realm access roles", KeycloakRoleParserAcceptsRealmAccessRoles()),
    ("keycloak role parser accepts resource access roles", KeycloakRoleParserAcceptsResourceAccessRoles()),
    ("keycloak role parser accepts simple roles claims", KeycloakRoleParserAcceptsSimpleRoleClaims()),
    ("common infrastructure has http current user", typeof(HttpCurrentUser).Name == nameof(HttpCurrentUser)),
    ("common infrastructure has http tenant context", typeof(HttpTenantContext).Name == nameof(HttpTenantContext)),
    ("current user exposes staff member id", SourceContains(
        "src/Common/ReserveFlow.Common.Application/Abstractions/ICurrentUser.cs",
        "Guid? StaffMemberId")),
    ("http current user reads staff member id claim", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/HttpCurrentUser.cs",
        "\"staff_member_id\"")),
    ("http current user reads keycloak claims", HttpCurrentUserReadsKeycloakClaims()),
    ("http tenant context reads tenant headers before claims", HttpTenantContextReadsTenantHeadersBeforeClaims()),
    ("common application has tenant slug resolver abstraction", HasTypeNamed(typeof(ITenantContext).Assembly, "ITenantSlugResolver")),
    ("common infrastructure has postgres tenant slug resolver", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/Tenancy/PostgresTenantSlugResolver.cs",
        "class PostgresTenantSlugResolver")),
    ("tenant slug resolver queries platform tenants by slug", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/Tenancy/PostgresTenantSlugResolver.cs",
        "from platform.tenants") &&
        SourceContains(
            "src/Common/ReserveFlow.Common.Infrastructure/Tenancy/PostgresTenantSlugResolver.cs",
            "slug = @tenant_slug")),
    ("infrastructure registers current user abstraction", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/InfrastructureConfiguration.cs",
        "AddScoped<ICurrentUser, HttpCurrentUser>")),
    ("infrastructure registers tenant context abstraction", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/InfrastructureConfiguration.cs",
        "AddScoped<ITenantContext, HttpTenantContext>")),
    ("infrastructure registers tenant slug resolver", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/InfrastructureConfiguration.cs",
        "ITenantSlugResolver")),
    ("database initializer can be configured to create missing database", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/Data/DatabaseInitializerOptions.cs",
        "CreateDatabaseIfMissing")),
    ("database initializer exposes maintenance database option", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/Data/DatabaseInitializerOptions.cs",
        "MaintenanceDatabase")),
    ("common infrastructure has postgres database bootstrapper", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/Data/PostgresDatabaseBootstrapper.cs",
        "class PostgresDatabaseBootstrapper")),
    ("postgres database bootstrapper uses maintenance connection", PostgresDatabaseBootstrapperUsesMaintenanceConnection()),
    ("database schema initializer creates database before schemas when enabled", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/Data/DatabaseSchemaInitializerHostedService.cs",
        "EnsureDatabaseExistsAsync")),
    ("docker compose enables database bootstrap for local stack", SourceContains(
        "docker-compose.yml",
        "DatabaseInitializer__CreateDatabaseIfMissing: \"true\"")),
    ("common application has tenant access guard abstraction", typeof(ITenantAccessGuard).Name == nameof(ITenantAccessGuard)),
    ("common infrastructure has tenant access guard", typeof(TenantAccessGuard).Name == nameof(TenantAccessGuard)),
    ("tenant access guard allows current tenant", TenantAccessGuardAllowsCurrentTenant()),
    ("tenant access guard allows platform scope", TenantAccessGuardAllowsPlatformScope()),
    ("tenant access guard denies different tenant", TenantAccessGuardDeniesDifferentTenant()),
    ("common presentation has tenant access endpoint filter", typeof(TenantAccessEndpointFilter).Name == nameof(TenantAccessEndpointFilter)),
    ("tenant admin endpoints with explicit tenant id require tenant access", TenantAdminTenantIdEndpointsRequireTenantAccess()),
    ("catalog id-only admin handlers enforce tenant ownership", CatalogIdOnlyAdminHandlersEnforceTenantOwnership()),
    ("staffing id-only admin handlers enforce tenant ownership", StaffingIdOnlyAdminHandlersEnforceTenantOwnership()),
    ("resources id-only admin handlers enforce tenant ownership", ResourcesIdOnlyAdminHandlersEnforceTenantOwnership()),
    ("booking id-only admin and staff handlers enforce tenant ownership", BookingIdOnlyHandlersEnforceTenantOwnership()),
    ("identity module has db context", typeof(IdentityDbContext).Name == nameof(IdentityDbContext)),
    ("identity domain has role entity", HasTypeNamed(typeof(User).Assembly, "Role")),
    ("identity domain has permission entity", HasTypeNamed(typeof(User).Assembly, "Permission")),
    ("identity domain has user role entity", HasTypeNamed(typeof(User).Assembly, "UserRole")),
    ("identity domain has role permission entity", HasTypeNamed(typeof(User).Assembly, "RolePermission")),
    ("role create normalizes role names", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Domain/Roles/Role.cs",
        "NormalizeRequired(name")),
    ("permission create normalizes permission codes", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Domain/Permissions/Permission.cs",
        "NormalizeRequired(code")),
    ("identity db context exposes roles", typeof(IdentityDbContext).GetProperty("Roles") is not null),
    ("identity db context exposes permissions", typeof(IdentityDbContext).GetProperty("Permissions") is not null),
    ("identity db context exposes user roles", typeof(IdentityDbContext).GetProperty("UserRoles") is not null),
    ("identity db context exposes role permissions", typeof(IdentityDbContext).GetProperty("RolePermissions") is not null),
    ("identity infrastructure maps roles", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Infrastructure/Database/IdentityDbContext.cs",
        "RoleConfiguration")),
    ("identity infrastructure maps permissions", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Infrastructure/Database/IdentityDbContext.cs",
        "PermissionConfiguration")),
    ("identity application has current user query", HasTypeNamed(typeof(ReserveFlow.Modules.Identity.Application.AssemblyReference).Assembly, "GetCurrentUserQuery")),
    ("identity application has current user response", HasTypeNamed(typeof(ReserveFlow.Modules.Identity.Application.AssemblyReference).Assembly, "CurrentUserResponse")),
    ("identity application has current user query handler", HasTypeNamed(typeof(ReserveFlow.Modules.Identity.Application.AssemblyReference).Assembly, "GetCurrentUserQueryHandler")),
    ("current user query combines keycloak and database permissions", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Application/CurrentUser/GetCurrentUserQueryHandler.cs",
        "GetPermissionsAsync")),
    ("identity application has user repository abstraction", HasTypeNamed(typeof(ReserveFlow.Modules.Identity.Application.AssemblyReference).Assembly, "IUserRepository")),
    ("identity application has permission reader abstraction", HasTypeNamed(typeof(ReserveFlow.Modules.Identity.Application.AssemblyReference).Assembly, "ICurrentUserPermissionReader")),
    ("identity infrastructure has user repository", HasTypeNamed(typeof(IdentityDbContext).Assembly, "UserRepository")),
    ("identity infrastructure has current user permission reader", HasTypeNamed(typeof(IdentityDbContext).Assembly, "CurrentUserPermissionReader")),
    ("identity module registers current user query handler", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Infrastructure/IdentityModule.cs",
        "GetCurrentUserQueryHandler")),
    ("identity module registers current user permission reader", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Infrastructure/IdentityModule.cs",
        "ICurrentUserPermissionReader")),
    ("current user permission reader joins role permissions", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Infrastructure/CurrentUser/CurrentUserPermissionReader.cs",
        "RolePermissions")),
    ("identity application has sync user command", HasTypeNamed(typeof(ReserveFlow.Modules.Identity.Application.AssemblyReference).Assembly, "SyncUserCommand")),
    ("identity application has sync user response", HasTypeNamed(typeof(ReserveFlow.Modules.Identity.Application.AssemblyReference).Assembly, "SyncUserResponse")),
    ("identity application has sync user command handler", HasTypeNamed(typeof(ReserveFlow.Modules.Identity.Application.AssemblyReference).Assembly, "SyncUserCommandHandler")),
    ("sync user command handler reads keycloak subject", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Application/Users/SyncUser/SyncUserCommandHandler.cs",
        "currentUser.KeycloakSubject")),
    ("sync user command handler saves changes", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Application/Users/SyncUser/SyncUserCommandHandler.cs",
        "SaveChangesAsync")),
    ("identity application has identity unit of work", HasTypeNamed(typeof(ReserveFlow.Modules.Identity.Application.AssemblyReference).Assembly, "IIdentityUnitOfWork")),
    ("identity user can update local profile", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Domain/Users/User.cs",
        "UpdateProfile")),
    ("identity user repository can insert users", typeof(ReserveFlow.Modules.Identity.Application.Users.IUserRepository).GetMethod("Insert") is not null),
    ("identity module registers sync user command handler", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Infrastructure/IdentityModule.cs",
        "SyncUserCommandHandler")),
    ("identity module registers identity unit of work", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Infrastructure/IdentityModule.cs",
        "IIdentityUnitOfWork")),
    ("identity application has assign tenant owner command", HasTypeNamed(typeof(ReserveFlow.Modules.Identity.Application.AssemblyReference).Assembly, "AssignTenantOwnerCommand")),
    ("identity application has assign tenant owner response", HasTypeNamed(typeof(ReserveFlow.Modules.Identity.Application.AssemblyReference).Assembly, "AssignTenantOwnerResponse")),
    ("identity application has assign tenant owner handler", HasTypeNamed(typeof(ReserveFlow.Modules.Identity.Application.AssemblyReference).Assembly, "AssignTenantOwnerCommandHandler")),
    ("identity application has tenant user repository abstraction", HasTypeNamed(typeof(ReserveFlow.Modules.Identity.Application.AssemblyReference).Assembly, "ITenantUserRepository")),
    ("assign tenant owner handler creates tenant admin membership", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Application/TenantUsers/AssignTenantOwner/AssignTenantOwnerCommandHandler.cs",
        "\"TenantAdmin\"")),
    ("assign tenant owner handler saves changes", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Application/TenantUsers/AssignTenantOwner/AssignTenantOwnerCommandHandler.cs",
        "SaveChangesAsync")),
    ("tenant user can update assigned role", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Domain/TenantUsers/TenantUser.cs",
        "AssignRole")),
    ("identity infrastructure has tenant user repository", HasTypeNamed(typeof(IdentityDbContext).Assembly, "TenantUserRepository")),
    ("identity module registers tenant user repository", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Infrastructure/IdentityModule.cs",
        "ITenantUserRepository")),
    ("identity module registers assign tenant owner handler", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Infrastructure/IdentityModule.cs",
        "AssignTenantOwnerCommandHandler")),
    ("tenants module has db context", typeof(TenantsDbContext).Name == nameof(TenantsDbContext)),
    ("tenants db context exposes tenant categories", typeof(TenantsDbContext).GetProperty("TenantCategories") is not null),
    ("catalog module has db context", typeof(CatalogDbContext).Name == nameof(CatalogDbContext)),
    ("staffing module has db context", typeof(StaffingDbContext).Name == nameof(StaffingDbContext)),
    ("resources module has db context", typeof(ResourcesDbContext).Name == nameof(ResourcesDbContext)),
    ("scheduling module has db context", typeof(SchedulingDbContext).Name == nameof(SchedulingDbContext)),
    ("scheduling db context exposes unavailable periods", typeof(SchedulingDbContext).GetProperty("UnavailablePeriods") is not null),
    ("bookings module has db context", typeof(BookingsDbContext).Name == nameof(BookingsDbContext)),
    ("notifications module has db context", typeof(NotificationsDbContext).Name == nameof(NotificationsDbContext)),
    ("audit module has db context", typeof(AuditDbContext).Name == nameof(AuditDbContext)),
    ("reporting module has db context", typeof(ReportingDbContext).Name == nameof(ReportingDbContext)),
    ("integrations module has db context", typeof(IntegrationsDbContext).Name == nameof(IntegrationsDbContext)),
    ("bookings db context exposes customers", typeof(BookingsDbContext).GetProperty("Customers") is not null),
    ("bookings db context exposes booking history", typeof(BookingsDbContext).GetProperty("BookingHistoryEntries") is not null),
    ("bookings db context exposes booking policies", typeof(BookingsDbContext).GetProperty("BookingPolicies") is not null),
    ("bookings db context exposes outbox", typeof(BookingsDbContext).GetProperty("OutboxMessages") is not null),
    ("common outbox message captures domain event", OutboxMessageCapturesDomainEvent()),
    ("common outbox has dispatcher", typeof(IOutboxMessageDispatcher).Name == nameof(IOutboxMessageDispatcher)),
    ("common outbox has logging dispatcher", typeof(LoggingOutboxMessageDispatcher).Name == nameof(LoggingOutboxMessageDispatcher)),
    ("bookings infrastructure has outbox processor", HasTypeNamed(typeof(BookingsDbContext).Assembly, "BookingsOutboxProcessorHostedService")),
    ("outbox message records processing outcome", OutboxMessageRecordsProcessingOutcome()),
    ("bookings module has module message response dto", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "ModuleMessageResponse")),
    ("bookings module has module message repository abstraction", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "IModuleMessageRepository")),
    ("bookings module has module messages query", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "GetModuleMessagesQuery")),
    ("bookings module has module messages query handler", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "GetModuleMessagesQueryHandler")),
    ("bookings infrastructure has module message repository", HasTypeNamed(typeof(BookingsDbContext).Assembly, "ModuleMessageRepository")),
    ("module message repository reads outbox messages by tenant", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Infrastructure/ModuleMessages/ModuleMessageRepository.cs",
        "message.TenantId == tenantId")),
    ("bookings presentation has admin module messages endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "GetModuleMessagesEndpoint")),
    ("admin module messages endpoint uses docs route", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/GetModuleMessagesEndpoint.cs",
        "\"/api/admin/module-messages\"")),
    ("admin module messages endpoint requires tenant admin", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/GetModuleMessagesEndpoint.cs",
        "RequireAuthorization(TenantAdminPolicy)")),
    ("admin module messages endpoint uses tenant context", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/GetModuleMessagesEndpoint.cs",
        "ITenantContext")),
    ("bookings module registers module messages query handler", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Infrastructure/BookingsModule.cs",
        "IQueryHandler<GetModuleMessagesQuery, IReadOnlyList<ModuleMessageResponse>>")),
    ("bookings module has create command", typeof(CreateBookingCommand).Name == nameof(CreateBookingCommand)),
    ("booking create command captures customer contact", HasPublicProperty(typeof(CreateBookingCommand), "CustomerEmail")),
    ("booking create command captures idempotency key", HasPublicProperty(typeof(CreateBookingCommand), "IdempotencyKey")),
    ("bookings module has create handler", typeof(CreateBookingCommandHandler).Name == nameof(CreateBookingCommandHandler)),
    ("booking repository can lookup idempotency key", typeof(IBookingRepository).GetMethod("FindByIdempotencyKeyAsync") is not null),
    ("bookings module has policy repository", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "IBookingPolicyRepository")),
    ("booking create handler checks policy", HasConstructorParameterNamed(typeof(CreateBookingCommandHandler), "IBookingPolicyRepository")),
    ("bookings module has availability checker contract", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "IBookingAvailabilityChecker")),
    ("booking create handler checks configured availability", HasConstructorParameterNamed(typeof(CreateBookingCommandHandler), "IBookingAvailabilityChecker")),
    ("bookings module has tenant booking gate contract", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "ITenantBookingGate")),
    ("booking create handler checks tenant booking gate", HasConstructorParameterNamed(typeof(CreateBookingCommandHandler), "ITenantBookingGate")),
    ("booking create handler blocks inactive public tenants", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Application/Bookings/CreateBooking/CreateBookingCommandHandler.cs",
        "Tenant cannot accept public bookings.")),
    ("bookings infrastructure has postgres tenant booking gate", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Infrastructure/Bookings/TenantStatus/PostgresTenantBookingGate.cs",
        "class PostgresTenantBookingGate")),
    ("tenant booking gate checks active platform tenant", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Infrastructure/Bookings/TenantStatus/PostgresTenantBookingGate.cs",
        "from platform.tenants") &&
        SourceContains(
            "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Infrastructure/Bookings/TenantStatus/PostgresTenantBookingGate.cs",
            "status = 'Active'")),
    ("bookings module registers tenant booking gate", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Infrastructure/BookingsModule.cs",
        "ITenantBookingGate")),
    ("booking cancel handler checks policy", HasConstructorParameterNamed(typeof(CancelBookingCommandHandler), "IBookingPolicyRepository")),
    ("booking cancel command captures policy enforcement", HasPublicProperty(typeof(CancelBookingCommand), "EnforcePolicy")),
    ("bookings module has configure policy command", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "ConfigureBookingPolicyCommand")),
    ("bookings module has configure policy handler", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "ConfigureBookingPolicyCommandHandler")),
    ("bookings module has customer repository", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "ICustomerRepository")),
    ("bookings module has history repository", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "IBookingHistoryRepository")),
    ("booking create handler records history", HasConstructorParameterNamed(typeof(CreateBookingCommandHandler), "IBookingHistoryRepository")),
    ("booking cancel handler records history", HasConstructorParameterNamed(typeof(CancelBookingCommandHandler), "IBookingHistoryRepository")),
    ("booking reschedule handler records history", HasConstructorParameterNamed(typeof(RescheduleBookingCommandHandler), "IBookingHistoryRepository")),
    ("booking reschedule handler checks configured availability", HasConstructorParameterNamed(typeof(RescheduleBookingCommandHandler), "IBookingAvailabilityChecker")),
    ("bookings infrastructure has scheduling-backed availability checker", HasTypeNamed(typeof(BookingsDbContext).Assembly, "SchedulingBookingAvailabilityChecker")),
    ("bookings module has cancel command", typeof(CancelBookingCommand).Name == nameof(CancelBookingCommand)),
    ("bookings module has cancel handler", typeof(CancelBookingCommandHandler).Name == nameof(CancelBookingCommandHandler)),
    ("bookings module has reschedule command", typeof(RescheduleBookingCommand).Name == nameof(RescheduleBookingCommand)),
    ("bookings module has reschedule handler", typeof(RescheduleBookingCommandHandler).Name == nameof(RescheduleBookingCommandHandler)),
    ("bookings module has confirm command", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "ConfirmBookingCommand")),
    ("bookings module has confirm handler", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "ConfirmBookingCommandHandler")),
    ("booking confirm handler records history", HasConstructorParameterNamed(
        typeof(ReserveFlow.Modules.Bookings.Application.Bookings.ConfirmBooking.ConfirmBookingCommandHandler),
        "IBookingHistoryRepository")),
    ("bookings module has expire command", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "ExpirePendingBookingCommand")),
    ("bookings module has expire handler", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "ExpirePendingBookingCommandHandler")),
    ("booking expire handler records history", HasConstructorParameterNamed(
        typeof(ReserveFlow.Modules.Bookings.Application.Bookings.ExpirePendingBooking.ExpirePendingBookingCommandHandler),
        "IBookingHistoryRepository")),
    ("bookings module has complete command", typeof(CompleteBookingCommand).Name == nameof(CompleteBookingCommand)),
    ("bookings module has complete handler", typeof(CompleteBookingCommandHandler).Name == nameof(CompleteBookingCommandHandler)),
    ("booking complete handler records history", HasConstructorParameterNamed(typeof(CompleteBookingCommandHandler), "IBookingHistoryRepository")),
    ("bookings presentation has admin complete endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "AdminCompleteBookingEndpoint")),
    ("admin complete endpoint uses admin route", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/AdminCompleteBookingEndpoint.cs",
        "\"/api/admin/bookings/{bookingId:guid}/complete\"")),
    ("admin complete endpoint requires tenant admin", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/AdminCompleteBookingEndpoint.cs",
        "RequireAuthorization(TenantAdminPolicy)")),
    ("admin complete endpoint uses complete command handler", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/AdminCompleteBookingEndpoint.cs",
        "CompleteBookingCommand")),
    ("bookings module has no-show command", typeof(MarkBookingAsNoShowCommand).Name == nameof(MarkBookingAsNoShowCommand)),
    ("bookings module has no-show handler", typeof(MarkBookingAsNoShowCommandHandler).Name == nameof(MarkBookingAsNoShowCommandHandler)),
    ("booking no-show handler records history", HasConstructorParameterNamed(typeof(MarkBookingAsNoShowCommandHandler), "IBookingHistoryRepository")),
    ("bookings presentation has admin no-show endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "AdminMarkBookingAsNoShowEndpoint")),
    ("admin no-show endpoint uses admin route", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/AdminMarkBookingAsNoShowEndpoint.cs",
        "\"/api/admin/bookings/{bookingId:guid}/no-show\"")),
    ("admin no-show endpoint requires tenant admin", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/AdminMarkBookingAsNoShowEndpoint.cs",
        "RequireAuthorization(TenantAdminPolicy)")),
    ("admin no-show endpoint uses no-show command handler", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/AdminMarkBookingAsNoShowEndpoint.cs",
        "MarkBookingAsNoShowCommand")),
    ("bookings module has response dto", typeof(BookingResponse).Name == nameof(BookingResponse)),
    ("booking response exposes concurrency token", HasPublicProperty(typeof(BookingResponse), "ConcurrencyToken")),
    ("booking repository can list bookings by tenant", typeof(IBookingRepository).GetMethod("GetByTenantIdAsync") is not null),
    ("booking repository can list bookings by tenant and staff member", typeof(IBookingRepository).GetMethod("GetByTenantIdAndStaffMemberIdAsync") is not null),
    ("booking repository can lookup booking by tenant and id", typeof(IBookingRepository).GetMethod("GetByTenantIdAndIdAsync") is not null),
    ("bookings module has admin bookings query", HasTypeNamed(typeof(BookingResponse).Assembly, "GetBookingsQuery")),
    ("bookings module has admin bookings query handler", HasTypeNamed(typeof(BookingResponse).Assembly, "GetBookingsQueryHandler")),
    ("bookings module has staff schedule query", HasTypeNamed(typeof(BookingResponse).Assembly, "GetStaffScheduleQuery")),
    ("bookings module has staff schedule query handler", HasTypeNamed(typeof(BookingResponse).Assembly, "GetStaffScheduleQueryHandler")),
    ("bookings module has admin booking detail query", HasTypeNamed(typeof(BookingResponse).Assembly, "GetBookingQuery")),
    ("bookings module has admin booking detail query handler", HasTypeNamed(typeof(BookingResponse).Assembly, "GetBookingQueryHandler")),
    ("bookings presentation has create endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "CreateBookingEndpoint")),
    ("bookings presentation has admin bookings endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "GetAdminBookingsEndpoint")),
    ("admin bookings endpoint uses admin route", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/GetAdminBookingsEndpoint.cs",
        "\"/api/admin/bookings\"")),
    ("admin bookings endpoint requires tenant admin", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/GetAdminBookingsEndpoint.cs",
        "RequireAuthorization(TenantAdminPolicy)")),
    ("admin bookings endpoint uses tenant context", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/GetAdminBookingsEndpoint.cs",
        "ITenantContext")),
    ("bookings presentation has admin booking detail endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "GetAdminBookingEndpoint")),
    ("admin booking detail endpoint uses admin route", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/GetAdminBookingEndpoint.cs",
        "\"/api/admin/bookings/{bookingId:guid}\"")),
    ("admin booking detail endpoint requires tenant admin", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/GetAdminBookingEndpoint.cs",
        "RequireAuthorization(TenantAdminPolicy)")),
    ("admin booking detail endpoint uses tenant context", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/GetAdminBookingEndpoint.cs",
        "ITenantContext")),
    ("bookings presentation has staff schedule endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "GetStaffScheduleEndpoint")),
    ("staff schedule endpoint uses staff route", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/GetStaffScheduleEndpoint.cs",
        "\"/api/staff/me/schedule\"")),
    ("staff schedule endpoint requires staff", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/GetStaffScheduleEndpoint.cs",
        "RequireAuthorization(StaffPolicy)")),
    ("staff schedule endpoint uses current user staff member id", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/GetStaffScheduleEndpoint.cs",
        "currentUser.StaffMemberId")),
    ("admin booking handlers use tenant scoped repository lookups", AdminBookingHandlersUseTenantScopedRepositoryLookups()),
    ("staff schedule handler uses tenant and staff scoped repository lookup", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Application/Bookings/GetStaffSchedule/GetStaffScheduleQueryHandler.cs",
        "GetByTenantIdAndStaffMemberIdAsync")),
    ("bookings module registers admin booking query handlers", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Infrastructure/BookingsModule.cs",
        "GetBookingsQueryHandler") &&
        SourceContains(
            "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Infrastructure/BookingsModule.cs",
            "GetBookingQueryHandler")),
    ("bookings module registers staff schedule query handler", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Infrastructure/BookingsModule.cs",
        "IQueryHandler<GetStaffScheduleQuery, IReadOnlyList<BookingResponse>>")),
    ("public booking create endpoint uses tenant slug route", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/CreateBookingEndpoint.cs",
        "\"/api/public/tenants/{tenantSlug}/bookings\"")),
    ("public booking create endpoint resolves tenant slug", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/CreateBookingEndpoint.cs",
        "ITenantSlugResolver")),
    ("public booking create endpoint returns not found for unknown tenant slug", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/CreateBookingEndpoint.cs",
        "TypedResults.NotFound()")),
    ("public booking request captures customer contact", HasTypeWithPublicProperty(
        ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly,
        "CreateBookingRequest",
        "CustomerEmail")),
    ("public booking request captures idempotency key", HasTypeWithPublicProperty(
        ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly,
        "CreateBookingRequest",
        "IdempotencyKey")),
    ("bookings presentation has configure policy endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "ConfigureBookingPolicyEndpoint")),
    ("bookings presentation has cancel endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "CancelBookingEndpoint")),
    ("public booking lookup endpoint uses tenant slug route", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/GetPublicBookingEndpoint.cs",
        "\"/api/public/tenants/{tenantSlug}/bookings/{publicReference}\"")),
    ("public booking lookup endpoint resolves tenant slug", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/GetPublicBookingEndpoint.cs",
        "ITenantSlugResolver")),
    ("public booking query captures tenant id", HasPublicProperty(
        typeof(ReserveFlow.Modules.Bookings.Application.Bookings.GetPublicBooking.GetPublicBookingQuery),
        "TenantId")),
    ("bookings presentation has public cancel endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "PublicCancelBookingEndpoint")),
    ("public cancel endpoint uses tenant slug route", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/PublicCancelBookingEndpoint.cs",
        "\"/api/public/tenants/{tenantSlug}/bookings/{publicReference}/cancel\"")),
    ("public cancel endpoint resolves tenant slug", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/PublicCancelBookingEndpoint.cs",
        "ITenantSlugResolver")),
    ("public cancel request captures access token", HasTypeWithPublicProperty(
        ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly,
        "PublicCancelBookingRequest",
        "AccessToken")),
    ("bookings module has public cancel command", HasTypeNamed(typeof(BookingResponse).Assembly, "CancelPublicBookingCommand")),
    ("public cancel command captures tenant id", HasPublicProperty(
        typeof(ReserveFlow.Modules.Bookings.Application.Bookings.CancelPublicBooking.CancelPublicBookingCommand),
        "TenantId")),
    ("bookings module has public cancel handler", HasTypeNamed(typeof(BookingResponse).Assembly, "CancelPublicBookingCommandHandler")),
    ("public cancel endpoint uses public lookup credentials", PublicCancelEndpointUsesPublicLookupCredentials()),
    ("public cancel handler uses public lookup and cancellation policy", PublicCancelHandlerUsesPublicLookupAndPolicy()),
    ("booking repository public lookup is tenant scoped", PublicBookingRepositoryLookupIsTenantScoped()),
    ("bookings presentation has reschedule endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "RescheduleBookingEndpoint")),
    ("bookings presentation has public reschedule endpoint", HasEndpointNamed(
        ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly,
        "PublicRescheduleBookingEndpoint")),
    ("public reschedule endpoint uses tenant slug route", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/PublicRescheduleBookingEndpoint.cs",
        "\"/api/public/tenants/{tenantSlug}/bookings/{publicReference}/reschedule\"")),
    ("public reschedule endpoint resolves tenant slug", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/PublicRescheduleBookingEndpoint.cs",
        "ITenantSlugResolver")),
    ("public reschedule request captures access token", HasTypeWithPublicProperty(
        ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly,
        "PublicRescheduleBookingRequest",
        "AccessToken")),
    ("bookings module has public reschedule command", HasTypeNamed(
        typeof(BookingResponse).Assembly,
        "PublicRescheduleBookingCommand")),
    ("public reschedule command captures tenant id", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Application/Bookings/PublicRescheduleBooking/PublicRescheduleBookingCommand.cs",
        "Guid TenantId")),
    ("bookings module has public reschedule handler", HasTypeNamed(
        typeof(BookingResponse).Assembly,
        "PublicRescheduleBookingCommandHandler")),
    ("public reschedule handler uses public lookup, policy, availability, overlap, and history", PublicRescheduleHandlerUsesPublicLookupPolicyAvailabilityAndHistory()),
    ("bookings module registers public reschedule handler", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Infrastructure/BookingsModule.cs",
        "PublicRescheduleBookingCommandHandler")),
    ("bookings presentation has confirm endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "ConfirmBookingEndpoint")),
    ("bookings presentation has expire endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "ExpirePendingBookingEndpoint")),
    ("bookings presentation has complete endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "CompleteBookingEndpoint")),
    ("bookings presentation has no-show endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "MarkBookingAsNoShowEndpoint")),
    ("notifications module has queue command", typeof(QueueNotificationCommand).Name == nameof(QueueNotificationCommand)),
    ("notifications module has queue handler", typeof(QueueNotificationCommandHandler).Name == nameof(QueueNotificationCommandHandler)),
    ("notifications module has response dto", typeof(NotificationResponse).Name == nameof(NotificationResponse)),
    ("notification repository can list tenant notifications", SourceContains(
        "src/Modules/Notifications/ReserveFlow.Modules.Notifications.Application/Notifications/INotificationRepository.cs",
        "GetByTenantIdAsync")),
    ("notifications module has tenant notifications query", HasTypeNamed(typeof(NotificationResponse).Assembly, "GetNotificationsQuery")),
    ("notifications module has tenant notifications query handler", HasTypeNamed(typeof(NotificationResponse).Assembly, "GetNotificationsQueryHandler")),
    ("notifications presentation has admin notifications endpoint", HasEndpointNamed(ReserveFlow.Modules.Notifications.Presentation.AssemblyReference.Assembly, "GetNotificationsEndpoint")),
    ("admin notifications endpoint uses docs route", SourceContains(
        "src/Modules/Notifications/ReserveFlow.Modules.Notifications.Presentation/GetNotificationsEndpoint.cs",
        "\"/api/admin/notifications\"")),
    ("admin notifications endpoint requires tenant admin", SourceContains(
        "src/Modules/Notifications/ReserveFlow.Modules.Notifications.Presentation/GetNotificationsEndpoint.cs",
        "RequireAuthorization(TenantAdminPolicy)")),
    ("admin notifications endpoint uses tenant context", SourceContains(
        "src/Modules/Notifications/ReserveFlow.Modules.Notifications.Presentation/GetNotificationsEndpoint.cs",
        "ITenantContext")),
    ("notifications module registers tenant notifications query handler", SourceContains(
        "src/Modules/Notifications/ReserveFlow.Modules.Notifications.Infrastructure/NotificationsModule.cs",
        "IQueryHandler<GetNotificationsQuery, IReadOnlyList<NotificationResponse>>")),
    ("notifications module has fake sender", typeof(FakeNotificationSender).Name == nameof(FakeNotificationSender)),
    ("notification queue normalizes data and raises domain event", NotificationQueueNormalizesDataAndRaisesDomainEvent()),
    ("audit module has record command", typeof(RecordAuditLogCommand).Name == nameof(RecordAuditLogCommand)),
    ("audit module has record handler", typeof(RecordAuditLogCommandHandler).Name == nameof(RecordAuditLogCommandHandler)),
    ("audit module has response dto", typeof(AuditLogResponse).Name == nameof(AuditLogResponse)),
    ("audit module has platform audit logs query", HasTypeNamed(typeof(AuditLogResponse).Assembly, "GetPlatformAuditLogsQuery")),
    ("audit module has platform audit logs query handler", HasTypeNamed(typeof(AuditLogResponse).Assembly, "GetPlatformAuditLogsQueryHandler")),
    ("audit repository can list recent logs", typeof(IAuditLogRepository).GetMethod("GetRecentAsync") is not null),
    ("audit presentation has platform audit logs endpoint", HasEndpointNamed(ReserveFlow.Modules.Audit.Presentation.AssemblyReference.Assembly, "GetPlatformAuditLogsEndpoint")),
    ("platform audit logs endpoint uses platform route", SourceContains(
        "src/Modules/Audit/ReserveFlow.Modules.Audit.Presentation/GetPlatformAuditLogsEndpoint.cs",
        "\"/api/platform/audit-logs\"")),
    ("platform audit logs endpoint requires platform admin", SourceContains(
        "src/Modules/Audit/ReserveFlow.Modules.Audit.Presentation/GetPlatformAuditLogsEndpoint.cs",
        "RequireAuthorization(PlatformAdminPolicy)")),
    ("audit module registers platform audit logs query handler", SourceContains(
        "src/Modules/Audit/ReserveFlow.Modules.Audit.Infrastructure/AuditModule.cs",
        "GetPlatformAuditLogsQueryHandler")),
    ("audit log normalizes data and raises domain event", AuditLogNormalizesDataAndRaisesDomainEvent()),
    ("reporting module has record command", typeof(RecordDailyBookingReportCommand).Name == nameof(RecordDailyBookingReportCommand)),
    ("reporting module has record handler", typeof(RecordDailyBookingReportCommandHandler).Name == nameof(RecordDailyBookingReportCommandHandler)),
    ("reporting module has daily query", typeof(GetDailyBookingReportQuery).Name == nameof(GetDailyBookingReportQuery)),
    ("reporting module has daily query handler", typeof(GetDailyBookingReportQueryHandler).Name == nameof(GetDailyBookingReportQueryHandler)),
    ("reporting module has response dto", typeof(DailyBookingReportResponse).Name == nameof(DailyBookingReportResponse)),
    ("reporting presentation has tenant daily report endpoint", HasEndpointNamed(ReserveFlow.Modules.Reporting.Presentation.AssemblyReference.Assembly, "GetTenantDailyReportEndpoint")),
    ("tenant daily report endpoint uses docs route", SourceContains(
        "src/Modules/Reporting/ReserveFlow.Modules.Reporting.Presentation/GetTenantDailyReportEndpoint.cs",
        "\"/api/admin/reports/daily\"")),
    ("tenant daily report endpoint requires tenant admin", SourceContains(
        "src/Modules/Reporting/ReserveFlow.Modules.Reporting.Presentation/GetTenantDailyReportEndpoint.cs",
        "RequireAuthorization(TenantAdminPolicy)")),
    ("tenant daily report endpoint uses tenant context", SourceContains(
        "src/Modules/Reporting/ReserveFlow.Modules.Reporting.Presentation/GetTenantDailyReportEndpoint.cs",
        "ITenantContext")),
    ("tenant daily report endpoint uses daily query handler", SourceContains(
        "src/Modules/Reporting/ReserveFlow.Modules.Reporting.Presentation/GetTenantDailyReportEndpoint.cs",
        "GetDailyBookingReportQuery")),
    ("reporting module has no-show response dto", HasTypeNamed(typeof(DailyBookingReportResponse).Assembly, "NoShowReportResponse")),
    ("reporting module has no-show query", HasTypeNamed(typeof(DailyBookingReportResponse).Assembly, "GetNoShowReportQuery")),
    ("reporting module has no-show query handler", HasTypeNamed(typeof(DailyBookingReportResponse).Assembly, "GetNoShowReportQueryHandler")),
    ("reporting presentation has tenant no-show report endpoint", HasEndpointNamed(ReserveFlow.Modules.Reporting.Presentation.AssemblyReference.Assembly, "GetNoShowReportEndpoint")),
    ("tenant no-show report endpoint uses docs route", SourceContains(
        "src/Modules/Reporting/ReserveFlow.Modules.Reporting.Presentation/GetNoShowReportEndpoint.cs",
        "\"/api/admin/reports/no-show\"")),
    ("tenant no-show report endpoint requires tenant admin", SourceContains(
        "src/Modules/Reporting/ReserveFlow.Modules.Reporting.Presentation/GetNoShowReportEndpoint.cs",
        "RequireAuthorization(TenantAdminPolicy)")),
    ("tenant no-show report endpoint uses tenant context", SourceContains(
        "src/Modules/Reporting/ReserveFlow.Modules.Reporting.Presentation/GetNoShowReportEndpoint.cs",
        "ITenantContext")),
    ("tenant no-show report endpoint uses no-show query handler", SourceContains(
        "src/Modules/Reporting/ReserveFlow.Modules.Reporting.Presentation/GetNoShowReportEndpoint.cs",
        "GetNoShowReportQuery")),
    ("reporting module registers no-show report query handler", SourceContains(
        "src/Modules/Reporting/ReserveFlow.Modules.Reporting.Infrastructure/ReportingModule.cs",
        "IQueryHandler<GetNoShowReportQuery, NoShowReportResponse?>")),
    ("daily booking report validates counts and raises domain event", DailyBookingReportValidatesCountsAndRaisesDomainEvent()),
    ("integrations module has accept webhook command", typeof(AcceptWebhookCommand).Name == nameof(AcceptWebhookCommand)),
    ("integrations module has accept webhook handler", typeof(AcceptWebhookCommandHandler).Name == nameof(AcceptWebhookCommandHandler)),
    ("integrations module has webhook inbox response dto", typeof(WebhookInboxMessageResponse).Name == nameof(WebhookInboxMessageResponse)),
    ("webhook inbox message normalizes data and raises domain event", WebhookInboxMessageNormalizesDataAndRaisesDomainEvent()),
    ("tenant create normalizes data and raises domain event", TenantCreateNormalizesDataAndRaisesDomainEvent()),
    ("tenant create can assign category", TenantCreateCanAssignCategory()),
    ("tenant response exposes category id", HasPublicProperty(typeof(TenantResponse), "CategoryId")),
    ("tenant category normalizes data and raises domain event", TenantCategoryCreateNormalizesDataAndRaisesDomainEvent()),
    ("tenants module has category repository", HasTypeNamed(typeof(TenantResponse).Assembly, "ITenantCategoryRepository")),
    ("tenants module has create category command", HasTypeNamed(typeof(TenantResponse).Assembly, "CreateTenantCategoryCommand")),
    ("tenants module has create category handler", HasTypeNamed(typeof(TenantResponse).Assembly, "CreateTenantCategoryCommandHandler")),
    ("tenants module has public category query", HasTypeNamed(typeof(TenantResponse).Assembly, "GetPublicTenantCategoriesQuery")),
    ("tenants module has platform category list query", HasTypeNamed(typeof(TenantResponse).Assembly, "GetPlatformTenantCategoriesQuery")),
    ("tenants module has platform category list query handler", HasTypeNamed(typeof(TenantResponse).Assembly, "GetPlatformTenantCategoriesQueryHandler")),
    ("tenants module has update category command", HasTypeNamed(typeof(TenantResponse).Assembly, "UpdateTenantCategoryCommand")),
    ("tenants module has update category handler", HasTypeNamed(typeof(TenantResponse).Assembly, "UpdateTenantCategoryCommandHandler")),
    ("tenants module has public tenant list query", HasTypeNamed(typeof(TenantResponse).Assembly, "GetPublicTenantsQuery")),
    ("tenants module has platform tenant list query", HasTypeNamed(typeof(TenantResponse).Assembly, "GetPlatformTenantsQuery")),
    ("tenants module has platform tenant list query handler", HasTypeNamed(typeof(TenantResponse).Assembly, "GetPlatformTenantsQueryHandler")),
    ("tenants module has platform tenant detail query", HasTypeNamed(typeof(TenantResponse).Assembly, "GetTenantQuery")),
    ("tenants module has platform tenant detail query handler", HasTypeNamed(typeof(TenantResponse).Assembly, "GetTenantQueryHandler")),
    ("tenants module has update tenant command", HasTypeNamed(typeof(TenantResponse).Assembly, "UpdateTenantCommand")),
    ("tenants module has update tenant handler", HasTypeNamed(typeof(TenantResponse).Assembly, "UpdateTenantCommandHandler")),
    ("tenants module has platform usage response", HasTypeNamed(typeof(TenantResponse).Assembly, "PlatformUsageResponse")),
    ("tenants module has platform usage query", HasTypeNamed(typeof(TenantResponse).Assembly, "GetPlatformUsageQuery")),
    ("tenants module has platform usage query handler", HasTypeNamed(typeof(TenantResponse).Assembly, "GetPlatformUsageQueryHandler")),
    ("tenants module has activate tenant command", HasTypeNamed(typeof(TenantResponse).Assembly, "ActivateTenantCommand")),
    ("tenants module has activate tenant handler", HasTypeNamed(typeof(TenantResponse).Assembly, "ActivateTenantCommandHandler")),
    ("tenants module has suspend tenant command", HasTypeNamed(typeof(TenantResponse).Assembly, "SuspendTenantCommand")),
    ("tenants module has suspend tenant handler", HasTypeNamed(typeof(TenantResponse).Assembly, "SuspendTenantCommandHandler")),
    ("tenant category repository can list platform categories", typeof(ITenantCategoryRepository).GetMethod("GetAllAsync") is not null),
    ("tenant category repository can lookup category by id", typeof(ITenantCategoryRepository).GetMethod("GetByIdAsync") is not null),
    ("tenant repository can list platform tenants", typeof(ITenantRepository).GetMethod("GetAllAsync") is not null),
    ("tenants presentation has public categories endpoint", HasEndpointNamed(ReserveFlow.Modules.Tenants.Presentation.AssemblyReference.Assembly, "GetPublicCategoriesEndpoint")),
    ("tenants presentation has platform categories endpoint", HasEndpointNamed(ReserveFlow.Modules.Tenants.Presentation.AssemblyReference.Assembly, "GetPlatformCategoriesEndpoint")),
    ("tenants presentation has update category endpoint", HasEndpointNamed(ReserveFlow.Modules.Tenants.Presentation.AssemblyReference.Assembly, "UpdateTenantCategoryEndpoint")),
    ("platform categories endpoint uses platform route", SourceContains(
        "src/Modules/Tenants/ReserveFlow.Modules.Tenants.Presentation/GetPlatformCategoriesEndpoint.cs",
        "\"/api/platform/categories\"")),
    ("update category endpoint uses platform route", SourceContains(
        "src/Modules/Tenants/ReserveFlow.Modules.Tenants.Presentation/UpdateTenantCategoryEndpoint.cs",
        "\"/api/platform/categories/{categoryId:guid}\"")),
    ("tenants presentation has public tenant list endpoint", HasEndpointNamed(ReserveFlow.Modules.Tenants.Presentation.AssemblyReference.Assembly, "GetPublicTenantsEndpoint")),
    ("tenants presentation has platform tenant list endpoint", HasEndpointNamed(ReserveFlow.Modules.Tenants.Presentation.AssemblyReference.Assembly, "GetPlatformTenantsEndpoint")),
    ("tenants presentation has platform tenant detail endpoint", HasEndpointNamed(ReserveFlow.Modules.Tenants.Presentation.AssemblyReference.Assembly, "GetPlatformTenantEndpoint")),
    ("tenants presentation has update tenant endpoint", HasEndpointNamed(ReserveFlow.Modules.Tenants.Presentation.AssemblyReference.Assembly, "UpdateTenantEndpoint")),
    ("tenants presentation has platform usage endpoint", HasEndpointNamed(ReserveFlow.Modules.Tenants.Presentation.AssemblyReference.Assembly, "GetPlatformUsageEndpoint")),
    ("platform tenant list endpoint uses platform route", SourceContains(
        "src/Modules/Tenants/ReserveFlow.Modules.Tenants.Presentation/GetPlatformTenantsEndpoint.cs",
        "\"/api/platform/tenants\"")),
    ("platform tenant detail endpoint uses platform route", SourceContains(
        "src/Modules/Tenants/ReserveFlow.Modules.Tenants.Presentation/GetPlatformTenantEndpoint.cs",
        "\"/api/platform/tenants/{tenantId:guid}\"")),
    ("update tenant endpoint uses platform route", SourceContains(
        "src/Modules/Tenants/ReserveFlow.Modules.Tenants.Presentation/UpdateTenantEndpoint.cs",
        "\"/api/platform/tenants/{tenantId:guid}\"")),
    ("platform usage endpoint uses platform route", SourceContains(
        "src/Modules/Tenants/ReserveFlow.Modules.Tenants.Presentation/GetPlatformUsageEndpoint.cs",
        "\"/api/platform/usage\"")),
    ("tenants presentation has create category endpoint", HasEndpointNamed(ReserveFlow.Modules.Tenants.Presentation.AssemblyReference.Assembly, "CreateTenantCategoryEndpoint")),
    ("tenants presentation has activate tenant endpoint", HasEndpointNamed(ReserveFlow.Modules.Tenants.Presentation.AssemblyReference.Assembly, "ActivateTenantEndpoint")),
    ("tenants presentation has suspend tenant endpoint", HasEndpointNamed(ReserveFlow.Modules.Tenants.Presentation.AssemblyReference.Assembly, "SuspendTenantEndpoint")),
    ("tenant category domain has updated event", HasTypeNamed(typeof(Tenant).Assembly, "TenantCategoryUpdatedDomainEvent")),
    ("tenant category update changes details and raises domain event", SourceContains(
        "src/Modules/Tenants/ReserveFlow.Modules.Tenants.Domain/TenantCategories/TenantCategory.cs",
        "TenantCategoryUpdatedDomainEvent")),
    ("tenants module registers platform category list query handler", SourceContains(
        "src/Modules/Tenants/ReserveFlow.Modules.Tenants.Infrastructure/TenantsModule.cs",
        "GetPlatformTenantCategoriesQueryHandler")),
    ("tenants module registers update category handler", SourceContains(
        "src/Modules/Tenants/ReserveFlow.Modules.Tenants.Infrastructure/TenantsModule.cs",
        "UpdateTenantCategoryCommandHandler")),
    ("tenant domain has updated event", HasTypeNamed(typeof(Tenant).Assembly, "TenantUpdatedDomainEvent")),
    ("tenant update changes details and raises domain event", SourceContains(
        "src/Modules/Tenants/ReserveFlow.Modules.Tenants.Domain/Tenants/Tenant.cs",
        "TenantUpdatedDomainEvent")),
    ("tenants module registers platform tenant list query handler", SourceContains(
        "src/Modules/Tenants/ReserveFlow.Modules.Tenants.Infrastructure/TenantsModule.cs",
        "GetPlatformTenantsQueryHandler")),
    ("tenants module registers update tenant handler", SourceContains(
        "src/Modules/Tenants/ReserveFlow.Modules.Tenants.Infrastructure/TenantsModule.cs",
        "UpdateTenantCommandHandler")),
    ("platform usage handler counts tenant statuses", SourceContains(
        "src/Modules/Tenants/ReserveFlow.Modules.Tenants.Application/Tenants/GetPlatformUsage/GetPlatformUsageQueryHandler.cs",
        "TenantStatus.Active")),
    ("tenants module registers platform usage query handler", SourceContains(
        "src/Modules/Tenants/ReserveFlow.Modules.Tenants.Infrastructure/TenantsModule.cs",
        "GetPlatformUsageQueryHandler")),
    ("tenant activate changes status and raises domain event", TenantActivateChangesStatusAndRaisesDomainEvent()),
    ("tenant suspend changes status and raises domain event", TenantSuspendChangesStatusAndRaisesDomainEvent()),
    ("service create normalizes data and raises domain event", ServiceCreateNormalizesDataAndRaisesDomainEvent()),
    ("catalog module has update service command", HasTypeNamed(typeof(ServiceResponse).Assembly, "UpdateServiceCommand")),
    ("catalog module has update service handler", HasTypeNamed(typeof(ServiceResponse).Assembly, "UpdateServiceCommandHandler")),
    ("catalog module has deactivate service command", HasTypeNamed(typeof(ServiceResponse).Assembly, "DeactivateServiceCommand")),
    ("catalog module has deactivate service handler", HasTypeNamed(typeof(ServiceResponse).Assembly, "DeactivateServiceCommandHandler")),
    ("catalog service repository can lookup service by id", typeof(IServiceRepository).GetMethod("GetByIdAsync") is not null),
    ("catalog service repository can list services by tenant", typeof(IServiceRepository).GetMethod("GetByTenantIdAsync") is not null),
    ("catalog service repository can lookup active service by tenant and id", typeof(IServiceRepository).GetMethod("GetActiveByIdAsync") is not null),
    ("catalog module has admin services query", HasTypeNamed(typeof(ServiceResponse).Assembly, "GetServicesQuery")),
    ("catalog module has admin services query handler", HasTypeNamed(typeof(ServiceResponse).Assembly, "GetServicesQueryHandler")),
    ("catalog module has admin service detail query", HasTypeNamed(typeof(ServiceResponse).Assembly, "GetServiceQuery")),
    ("catalog module has admin service detail query handler", HasTypeNamed(typeof(ServiceResponse).Assembly, "GetServiceQueryHandler")),
    ("catalog module has public active service detail query", HasTypeNamed(typeof(ServiceResponse).Assembly, "GetActiveServiceQuery")),
    ("catalog module has public active service detail handler", HasTypeNamed(typeof(ServiceResponse).Assembly, "GetActiveServiceQueryHandler")),
    ("catalog presentation has admin services endpoint", HasEndpointNamed(ReserveFlow.Modules.Catalog.Presentation.AssemblyReference.Assembly, "GetAdminServicesEndpoint")),
    ("catalog presentation has admin service detail endpoint", HasEndpointNamed(ReserveFlow.Modules.Catalog.Presentation.AssemblyReference.Assembly, "GetAdminServiceEndpoint")),
    ("catalog presentation has update service endpoint", HasEndpointNamed(ReserveFlow.Modules.Catalog.Presentation.AssemblyReference.Assembly, "UpdateServiceEndpoint")),
    ("catalog presentation has deactivate service endpoint", HasEndpointNamed(ReserveFlow.Modules.Catalog.Presentation.AssemblyReference.Assembly, "DeactivateServiceEndpoint")),
    ("public services endpoint uses tenant slug route", SourceContains(
        "src/Modules/Catalog/ReserveFlow.Modules.Catalog.Presentation/GetTenantServicesEndpoint.cs",
        "\"/api/public/tenants/{tenantSlug}/services\"")),
    ("public services endpoint resolves tenant slug", SourceContains(
        "src/Modules/Catalog/ReserveFlow.Modules.Catalog.Presentation/GetTenantServicesEndpoint.cs",
        "ITenantSlugResolver")),
    ("public services endpoint returns not found for unknown tenant slug", SourceContains(
        "src/Modules/Catalog/ReserveFlow.Modules.Catalog.Presentation/GetTenantServicesEndpoint.cs",
        "TypedResults.NotFound()")),
    ("catalog presentation has public service detail endpoint", HasEndpointNamed(ReserveFlow.Modules.Catalog.Presentation.AssemblyReference.Assembly, "GetTenantServiceEndpoint")),
    ("public service detail endpoint uses tenant slug route", SourceContains(
        "src/Modules/Catalog/ReserveFlow.Modules.Catalog.Presentation/GetTenantServiceEndpoint.cs",
        "\"/api/public/tenants/{tenantSlug}/services/{serviceId:guid}\"")),
    ("public service detail endpoint resolves tenant slug", SourceContains(
        "src/Modules/Catalog/ReserveFlow.Modules.Catalog.Presentation/GetTenantServiceEndpoint.cs",
        "ITenantSlugResolver")),
    ("public service detail endpoint returns not found for unknown tenant slug or inactive service", SourceContains(
        "src/Modules/Catalog/ReserveFlow.Modules.Catalog.Presentation/GetTenantServiceEndpoint.cs",
        "TypedResults.NotFound()")),
    ("public active service detail handler uses active tenant scoped lookup", SourceContains(
        "src/Modules/Catalog/ReserveFlow.Modules.Catalog.Application/Services/GetActiveService/GetActiveServiceQueryHandler.cs",
        "GetActiveByIdAsync")),
    ("catalog module registers public active service detail query handler", SourceContains(
        "src/Modules/Catalog/ReserveFlow.Modules.Catalog.Infrastructure/CatalogModule.cs",
        "GetActiveServiceQueryHandler")),
    ("service update changes details and raises domain event", ServiceUpdateChangesDetailsAndRaisesDomainEvent()),
    ("service deactivate changes active flag and raises domain event", ServiceDeactivateChangesActiveFlagAndRaisesDomainEvent()),
    ("staff member create normalizes data and raises domain event", StaffMemberCreateNormalizesDataAndRaisesDomainEvent()),
    ("staffing module has update staff member command", HasTypeNamed(typeof(StaffMemberResponse).Assembly, "UpdateStaffMemberCommand")),
    ("staffing module has update staff member handler", HasTypeNamed(typeof(StaffMemberResponse).Assembly, "UpdateStaffMemberCommandHandler")),
    ("staffing module has deactivate staff member command", HasTypeNamed(typeof(StaffMemberResponse).Assembly, "DeactivateStaffMemberCommand")),
    ("staffing module has deactivate staff member handler", HasTypeNamed(typeof(StaffMemberResponse).Assembly, "DeactivateStaffMemberCommandHandler")),
    ("staff member repository can lookup staff member by id", typeof(IStaffMemberRepository).GetMethod("GetByIdAsync") is not null),
    ("staff member repository can list staff members by tenant", typeof(IStaffMemberRepository).GetMethod("GetByTenantIdAsync") is not null),
    ("staffing module has admin staff members query", HasTypeNamed(typeof(StaffMemberResponse).Assembly, "GetStaffMembersQuery")),
    ("staffing module has admin staff members query handler", HasTypeNamed(typeof(StaffMemberResponse).Assembly, "GetStaffMembersQueryHandler")),
    ("staffing module has admin staff member detail query", HasTypeNamed(typeof(StaffMemberResponse).Assembly, "GetStaffMemberQuery")),
    ("staffing module has admin staff member detail query handler", HasTypeNamed(typeof(StaffMemberResponse).Assembly, "GetStaffMemberQueryHandler")),
    ("staffing presentation has admin staff members endpoint", HasEndpointNamed(ReserveFlow.Modules.Staffing.Presentation.AssemblyReference.Assembly, "GetAdminStaffMembersEndpoint")),
    ("staffing presentation has admin staff member detail endpoint", HasEndpointNamed(ReserveFlow.Modules.Staffing.Presentation.AssemblyReference.Assembly, "GetAdminStaffMemberEndpoint")),
    ("staffing presentation has update staff member endpoint", HasEndpointNamed(ReserveFlow.Modules.Staffing.Presentation.AssemblyReference.Assembly, "UpdateStaffMemberEndpoint")),
    ("staffing presentation has deactivate staff member endpoint", HasEndpointNamed(ReserveFlow.Modules.Staffing.Presentation.AssemblyReference.Assembly, "DeactivateStaffMemberEndpoint")),
    ("staff member update changes details and raises domain event", StaffMemberUpdateChangesDetailsAndRaisesDomainEvent()),
    ("staff member deactivate changes active flag and raises domain event", StaffMemberDeactivateChangesActiveFlagAndRaisesDomainEvent()),
    ("resource create normalizes data and raises domain event", ResourceCreateNormalizesDataAndRaisesDomainEvent()),
    ("resources module has update resource command", HasTypeNamed(typeof(ResourceResponse).Assembly, "UpdateResourceCommand")),
    ("resources module has update resource handler", HasTypeNamed(typeof(ResourceResponse).Assembly, "UpdateResourceCommandHandler")),
    ("resources module has deactivate resource command", HasTypeNamed(typeof(ResourceResponse).Assembly, "DeactivateResourceCommand")),
    ("resources module has deactivate resource handler", HasTypeNamed(typeof(ResourceResponse).Assembly, "DeactivateResourceCommandHandler")),
    ("resource repository can lookup resource by id", typeof(IResourceRepository).GetMethod("GetByIdAsync") is not null),
    ("resource repository can list resources by tenant", typeof(IResourceRepository).GetMethod("GetByTenantIdAsync") is not null),
    ("resources module has admin resources query", HasTypeNamed(typeof(ResourceResponse).Assembly, "GetResourcesQuery")),
    ("resources module has admin resources query handler", HasTypeNamed(typeof(ResourceResponse).Assembly, "GetResourcesQueryHandler")),
    ("resources module has admin resource detail query", HasTypeNamed(typeof(ResourceResponse).Assembly, "GetResourceQuery")),
    ("resources module has admin resource detail query handler", HasTypeNamed(typeof(ResourceResponse).Assembly, "GetResourceQueryHandler")),
    ("resources presentation has admin resources endpoint", HasEndpointNamed(ReserveFlow.Modules.Resources.Presentation.AssemblyReference.Assembly, "GetAdminResourcesEndpoint")),
    ("resources presentation has admin resource detail endpoint", HasEndpointNamed(ReserveFlow.Modules.Resources.Presentation.AssemblyReference.Assembly, "GetAdminResourceEndpoint")),
    ("resources presentation has update resource endpoint", HasEndpointNamed(ReserveFlow.Modules.Resources.Presentation.AssemblyReference.Assembly, "UpdateResourceEndpoint")),
    ("resources presentation has deactivate resource endpoint", HasEndpointNamed(ReserveFlow.Modules.Resources.Presentation.AssemblyReference.Assembly, "DeactivateResourceEndpoint")),
    ("resource update changes details and raises domain event", ResourceUpdateChangesDetailsAndRaisesDomainEvent()),
    ("resource deactivate changes active flag and raises domain event", ResourceDeactivateChangesActiveFlagAndRaisesDomainEvent()),
    ("scheduling module has working hour response", HasTypeNamed(typeof(AvailableSlotResponse).Assembly, "WorkingHourResponse")),
    ("scheduling module has create working hour command", HasTypeNamed(typeof(AvailableSlotResponse).Assembly, "CreateWorkingHourCommand")),
    ("scheduling module has create working hour handler", HasTypeNamed(typeof(AvailableSlotResponse).Assembly, "CreateWorkingHourCommandHandler")),
    ("working hour repository can query by target and day", typeof(IWorkingHourRepository).GetMethod("GetByTargetAndDayAsync") is not null),
    ("scheduling module has tenant availability query", HasTypeNamed(typeof(AvailableSlotResponse).Assembly, "GetTenantAvailableSlotsQuery")),
    ("scheduling module has tenant availability handler", HasTypeNamed(typeof(AvailableSlotResponse).Assembly, "GetTenantAvailableSlotsQueryHandler")),
    ("scheduling module has unavailable period response", HasTypeNamed(typeof(AvailableSlotResponse).Assembly, "UnavailablePeriodResponse")),
    ("scheduling module has unavailable period repository", HasTypeNamed(typeof(AvailableSlotResponse).Assembly, "IUnavailablePeriodRepository")),
    ("scheduling module has create unavailable period command", HasTypeNamed(typeof(AvailableSlotResponse).Assembly, "CreateUnavailablePeriodCommand")),
    ("scheduling module has create unavailable period handler", HasTypeNamed(typeof(AvailableSlotResponse).Assembly, "CreateUnavailablePeriodCommandHandler")),
    ("tenant availability handler subtracts unavailable periods", HasConstructorParameterNamed(typeof(GetTenantAvailableSlotsQueryHandler), "IUnavailablePeriodRepository")),
    ("scheduling presentation has create staff working hour endpoint", HasEndpointNamed(ReserveFlow.Modules.Scheduling.Presentation.AssemblyReference.Assembly, "CreateStaffWorkingHourEndpoint")),
    ("scheduling presentation has create resource working hour endpoint", HasEndpointNamed(ReserveFlow.Modules.Scheduling.Presentation.AssemblyReference.Assembly, "CreateResourceWorkingHourEndpoint")),
    ("scheduling presentation has create staff unavailable period endpoint", HasEndpointNamed(ReserveFlow.Modules.Scheduling.Presentation.AssemblyReference.Assembly, "CreateStaffUnavailablePeriodEndpoint")),
    ("scheduling presentation has create resource unavailable period endpoint", HasEndpointNamed(ReserveFlow.Modules.Scheduling.Presentation.AssemblyReference.Assembly, "CreateResourceUnavailablePeriodEndpoint")),
    ("scheduling presentation has staff own unavailable period endpoint", HasEndpointNamed(ReserveFlow.Modules.Scheduling.Presentation.AssemblyReference.Assembly, "CreateOwnStaffUnavailablePeriodEndpoint")),
    ("staff own unavailable period endpoint uses staff route", SourceContains(
        "src/Modules/Scheduling/ReserveFlow.Modules.Scheduling.Presentation/CreateOwnStaffUnavailablePeriodEndpoint.cs",
        "\"/api/staff/unavailable-periods\"")),
    ("staff own unavailable period endpoint requires staff", SourceContains(
        "src/Modules/Scheduling/ReserveFlow.Modules.Scheduling.Presentation/CreateOwnStaffUnavailablePeriodEndpoint.cs",
        "RequireAuthorization(StaffPolicy)")),
    ("staff own unavailable period endpoint uses tenant context", SourceContains(
        "src/Modules/Scheduling/ReserveFlow.Modules.Scheduling.Presentation/CreateOwnStaffUnavailablePeriodEndpoint.cs",
        "ITenantContext")),
    ("staff own unavailable period endpoint uses current user staff member id", SourceContains(
        "src/Modules/Scheduling/ReserveFlow.Modules.Scheduling.Presentation/CreateOwnStaffUnavailablePeriodEndpoint.cs",
        "currentUser.StaffMemberId")),
    ("staff own unavailable period request does not accept tenant id", SourceDoesNotContain(
        "src/Modules/Scheduling/ReserveFlow.Modules.Scheduling.Presentation/CreateOwnUnavailablePeriodRequest.cs",
        "TenantId")),
    ("scheduling presentation has tenant availability endpoint", HasEndpointNamed(ReserveFlow.Modules.Scheduling.Presentation.AssemblyReference.Assembly, "GetTenantAvailableSlotsEndpoint")),
    ("public availability endpoint uses tenant slug route", SourceContains(
        "src/Modules/Scheduling/ReserveFlow.Modules.Scheduling.Presentation/GetTenantAvailableSlotsEndpoint.cs",
        "\"/api/public/tenants/{tenantSlug}/availability\"")),
    ("public availability endpoint resolves tenant slug", SourceContains(
        "src/Modules/Scheduling/ReserveFlow.Modules.Scheduling.Presentation/GetTenantAvailableSlotsEndpoint.cs",
        "ITenantSlugResolver")),
    ("public availability endpoint returns not found for unknown tenant slug", SourceContains(
        "src/Modules/Scheduling/ReserveFlow.Modules.Scheduling.Presentation/GetTenantAvailableSlotsEndpoint.cs",
        "TypedResults.NotFound()")),
    ("working hour create targets staff or resource and raises domain event", WorkingHourCreateTargetsStaffOrResourceAndRaisesDomainEvent()),
    ("unavailable period create targets staff or resource and raises domain event", UnavailablePeriodCreateTargetsStaffOrResourceAndRaisesDomainEvent()),
    ("booking availability checker excludes unavailable periods", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Infrastructure/Bookings/Availability/SchedulingBookingAvailabilityChecker.cs",
        "unavailable_periods")),
    ("availability engine generates fixed-duration slots", AvailabilityEngineGeneratesFixedDurationSlots()),
    ("booking create raises domain event", BookingCreateRaisesDomainEvent()),
    ("booking create stores idempotency key", BookingCreateStoresIdempotencyKey()),
    ("booking create generates public lookup credentials", BookingCreateGeneratesPublicLookupCredentials()),
    ("booking response exposes public lookup credentials", HasPublicProperty(typeof(BookingResponse), "PublicReference") &&
                                                             HasPublicProperty(typeof(BookingResponse), "AccessToken")),
    ("booking repository can lookup public booking", typeof(IBookingRepository).GetMethod("FindByPublicLookupAsync") is not null),
    ("bookings module has public booking lookup query", HasTypeNamed(typeof(BookingResponse).Assembly, "GetPublicBookingQuery")),
    ("bookings module has public booking lookup handler", HasTypeNamed(typeof(BookingResponse).Assembly, "GetPublicBookingQueryHandler")),
    ("bookings presentation has public booking lookup endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "GetPublicBookingEndpoint")),
    ("booking exposes optimistic concurrency token", BookingExposesOptimisticConcurrencyToken()),
    ("booking lifecycle mutation changes concurrency token", BookingLifecycleMutationChangesConcurrencyToken()),
    ("customer register normalizes data and raises domain event", CustomerRegisterNormalizesDataAndRaisesDomainEvent()),
    ("booking history records status and raises domain event", BookingHistoryRecordsStatusAndRaisesDomainEvent()),
    ("booking policy configures rules and raises domain event", BookingPolicyConfiguresRulesAndRaisesDomainEvent()),
    ("booking policy blocks booking inside minimum advance window", BookingPolicyBlocksBookingInsideMinimumAdvanceWindow()),
    ("booking policy blocks cancellation after deadline", BookingPolicyBlocksCancellationAfterDeadline()),
    ("booking cancel changes status and raises domain event", BookingCancelChangesStatusAndRaisesDomainEvent()),
    ("booking reschedule changes time and raises domain event", BookingRescheduleChangesTimeAndRaisesDomainEvent()),
    ("booking confirm changes status and raises domain event", BookingConfirmChangesStatusAndRaisesDomainEvent()),
    ("booking expire changes status and raises domain event", BookingExpireChangesStatusAndRaisesDomainEvent()),
    ("booking complete changes status and raises domain event", BookingCompleteChangesStatusAndRaisesDomainEvent()),
    ("booking no-show changes status and raises domain event", BookingNoShowChangesStatusAndRaisesDomainEvent())
};

foreach ((string name, bool passed) in checks)
{
    Console.WriteLine($"{(passed ? "PASS" : "FAIL")} {name}");
}

if (checks.Exists(check => !check.Passed))
{
    Environment.ExitCode = 1;
}

static bool HasEndpoint(System.Reflection.Assembly assembly)
{
    return assembly
        .GetTypes()
        .Any(type => type is { IsAbstract: false, IsInterface: false } &&
                     typeof(IEndpoint).IsAssignableFrom(type));
}

static bool HasEndpointNamed(System.Reflection.Assembly assembly, string typeName)
{
    return assembly
        .GetTypes()
        .Any(type => type.Name == typeName &&
                     type is { IsAbstract: false, IsInterface: false } &&
                     typeof(IEndpoint).IsAssignableFrom(type));
}

static bool HasTypeNamed(System.Reflection.Assembly assembly, string typeName)
{
    return assembly.GetTypes().Any(type => type.Name == typeName);
}

static bool HasTypeWithPublicProperty(
    System.Reflection.Assembly assembly,
    string typeName,
    string propertyName)
{
    return assembly
        .GetTypes()
        .Any(type => type.Name == typeName && HasPublicProperty(type, propertyName));
}

static bool HasPublicProperty(Type type, string propertyName)
{
    return type.GetProperty(propertyName) is not null;
}

static bool SourceContains(string relativePath, string expectedText)
{
    string path = Path.Combine(Directory.GetCurrentDirectory(), relativePath.Replace('/', Path.DirectorySeparatorChar));

    return File.Exists(path) &&
           File.ReadAllText(path).Contains(expectedText, StringComparison.Ordinal);
}

static bool SourceDoesNotContain(string relativePath, string unexpectedText)
{
    string path = Path.Combine(Directory.GetCurrentDirectory(), relativePath.Replace('/', Path.DirectorySeparatorChar));

    return File.Exists(path) &&
           !File.ReadAllText(path).Contains(unexpectedText, StringComparison.Ordinal);
}

static bool HasConstructorParameterNamed(Type type, string parameterTypeName)
{
    return type.GetConstructors()
        .SelectMany(constructor => constructor.GetParameters())
        .Any(parameter => parameter.ParameterType.Name == parameterTypeName);
}

static bool BookingCreateRaisesDomainEvent()
{
    Booking booking = Booking.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        staffMemberId: null,
        resourceId: Guid.NewGuid(),
        DateTimeOffset.UtcNow.AddHours(1),
        DateTimeOffset.UtcNow.AddHours(2));

    return booking.Status == BookingStatus.Pending &&
           booking.DomainEvents.OfType<BookingCreatedDomainEvent>().Any();
}

static bool BookingCreateStoresIdempotencyKey()
{
    System.Reflection.MethodInfo? createMethod = typeof(Booking).GetMethod(
        "Create",
        [
            typeof(Guid),
            typeof(Guid),
            typeof(Guid),
            typeof(Guid?),
            typeof(Guid?),
            typeof(DateTimeOffset),
            typeof(DateTimeOffset),
            typeof(string)
        ]);

    if (createMethod is null)
    {
        return false;
    }

    object? booking = createMethod.Invoke(
        null,
        [
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(2),
            "  public-retry-key  "
        ]);

    return booking is Booking typedBooking &&
           typeof(Booking).GetProperty("IdempotencyKey")?.GetValue(typedBooking) as string == "public-retry-key";
}

static bool BookingCreateGeneratesPublicLookupCredentials()
{
    Booking booking = Booking.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        staffMemberId: Guid.NewGuid(),
        resourceId: null,
        DateTimeOffset.UtcNow.AddHours(1),
        DateTimeOffset.UtcNow.AddHours(2));

    string? publicReference = typeof(Booking).GetProperty("PublicReference")?.GetValue(booking) as string;
    string? accessToken = typeof(Booking).GetProperty("AccessToken")?.GetValue(booking) as string;

    return !string.IsNullOrWhiteSpace(publicReference) &&
           publicReference.StartsWith("RF-", StringComparison.Ordinal) &&
           !string.IsNullOrWhiteSpace(accessToken) &&
           accessToken.Length >= 32;
}

static bool BookingExposesOptimisticConcurrencyToken()
{
    Booking booking = Booking.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        staffMemberId: Guid.NewGuid(),
        resourceId: null,
        DateTimeOffset.UtcNow.AddHours(1),
        DateTimeOffset.UtcNow.AddHours(2));

    object? concurrencyToken = typeof(Booking).GetProperty("ConcurrencyToken")?.GetValue(booking);

    return concurrencyToken is Guid token && token != Guid.Empty;
}

static bool BookingLifecycleMutationChangesConcurrencyToken()
{
    Booking booking = Booking.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        staffMemberId: Guid.NewGuid(),
        resourceId: null,
        DateTimeOffset.UtcNow.AddHours(1),
        DateTimeOffset.UtcNow.AddHours(2));

    System.Reflection.PropertyInfo? property = typeof(Booking).GetProperty("ConcurrencyToken");

    if (property?.GetValue(booking) is not Guid originalToken)
    {
        return false;
    }

    booking.ClearDomainEvents();
    booking.Cancel(DateTimeOffset.UtcNow);

    return property.GetValue(booking) is Guid changedToken &&
           changedToken != Guid.Empty &&
           changedToken != originalToken;
}

static bool CustomerRegisterNormalizesDataAndRaisesDomainEvent()
{
    Type? customerType = typeof(Booking).Assembly.GetType(
        "ReserveFlow.Modules.Bookings.Domain.Customers.Customer");
    Type? registeredEventType = typeof(Booking).Assembly.GetType(
        "ReserveFlow.Modules.Bookings.Domain.Customers.CustomerRegisteredDomainEvent");

    if (customerType is null || registeredEventType is null)
    {
        return false;
    }

    System.Reflection.MethodInfo? registerMethod = customerType.GetMethod(
        "Register",
        [typeof(Guid), typeof(string), typeof(string), typeof(string)]);

    if (registerMethod is null)
    {
        return false;
    }

    object? customer = registerMethod.Invoke(
        null,
        [Guid.NewGuid(), "  Ali Valiyev  ", "  CUSTOMER@EXAMPLE.COM  ", " +998 90 123 45 67 "]);

    if (customer is not Entity entity)
    {
        return false;
    }

    return customerType.GetProperty("FullName")?.GetValue(customer) as string == "Ali Valiyev" &&
           customerType.GetProperty("Email")?.GetValue(customer) as string == "customer@example.com" &&
           customerType.GetProperty("PhoneNumber")?.GetValue(customer) as string == "+998 90 123 45 67" &&
           entity.DomainEvents.Any(registeredEventType.IsInstanceOfType);
}

static bool BookingHistoryRecordsStatusAndRaisesDomainEvent()
{
    Type? historyType = typeof(Booking).Assembly.GetType(
        "ReserveFlow.Modules.Bookings.Domain.BookingHistory.BookingHistoryEntry");
    Type? recordedEventType = typeof(Booking).Assembly.GetType(
        "ReserveFlow.Modules.Bookings.Domain.BookingHistory.BookingHistoryRecordedDomainEvent");

    if (historyType is null || recordedEventType is null)
    {
        return false;
    }

    System.Reflection.MethodInfo? recordMethod = historyType.GetMethod(
        "Record",
        [typeof(Guid), typeof(Guid), typeof(BookingStatus), typeof(DateTimeOffset), typeof(string)]);

    if (recordMethod is null)
    {
        return false;
    }

    Guid tenantId = Guid.NewGuid();
    Guid bookingId = Guid.NewGuid();
    DateTimeOffset changedAtUtc = DateTimeOffset.UtcNow;

    object? history = recordMethod.Invoke(
        null,
        [tenantId, bookingId, BookingStatus.Confirmed, changedAtUtc, "  Confirmed by admin  "]);

    if (history is not Entity entity)
    {
        return false;
    }

    return historyType.GetProperty("TenantId")?.GetValue(history) is Guid returnedTenantId &&
           returnedTenantId == tenantId &&
           historyType.GetProperty("BookingId")?.GetValue(history) is Guid returnedBookingId &&
           returnedBookingId == bookingId &&
           historyType.GetProperty("Status")?.GetValue(history) is BookingStatus status &&
           status == BookingStatus.Confirmed &&
           historyType.GetProperty("Reason")?.GetValue(history) as string == "Confirmed by admin" &&
           entity.DomainEvents.Any(recordedEventType.IsInstanceOfType);
}

static bool BookingPolicyConfiguresRulesAndRaisesDomainEvent()
{
    Type? policyType = typeof(Booking).Assembly.GetType(
        "ReserveFlow.Modules.Bookings.Domain.BookingPolicies.BookingPolicy");
    Type? configuredEventType = typeof(Booking).Assembly.GetType(
        "ReserveFlow.Modules.Bookings.Domain.BookingPolicies.BookingPolicyConfiguredDomainEvent");

    if (policyType is null || configuredEventType is null)
    {
        return false;
    }

    System.Reflection.MethodInfo? configureMethod = policyType.GetMethod(
        "Configure",
        [typeof(Guid), typeof(int), typeof(int)]);

    if (configureMethod is null)
    {
        return false;
    }

    Guid tenantId = Guid.NewGuid();
    object? policy = configureMethod.Invoke(null, [tenantId, 120, 12]);

    if (policy is not Entity entity)
    {
        return false;
    }

    return policyType.GetProperty("TenantId")?.GetValue(policy) is Guid returnedTenantId &&
           returnedTenantId == tenantId &&
           policyType.GetProperty("MinimumAdvanceMinutes")?.GetValue(policy) is int minimumAdvanceMinutes &&
           minimumAdvanceMinutes == 120 &&
           policyType.GetProperty("CancellationDeadlineHours")?.GetValue(policy) is int cancellationDeadlineHours &&
           cancellationDeadlineHours == 12 &&
           entity.DomainEvents.Any(configuredEventType.IsInstanceOfType);
}

static bool BookingPolicyBlocksBookingInsideMinimumAdvanceWindow()
{
    object? policy = CreateBookingPolicy(minimumAdvanceMinutes: 120, cancellationDeadlineHours: 12);

    if (policy is null)
    {
        return false;
    }

    System.Reflection.MethodInfo? ensureMethod = policy.GetType().GetMethod(
        "EnsureBookingCanStartAt",
        [typeof(DateTimeOffset), typeof(DateTimeOffset)]);

    if (ensureMethod is null)
    {
        return false;
    }

    try
    {
        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        ensureMethod.Invoke(policy, [nowUtc.AddMinutes(90), nowUtc]);
        return false;
    }
    catch (System.Reflection.TargetInvocationException exception)
        when (exception.InnerException is InvalidOperationException)
    {
        return true;
    }
}

static bool BookingPolicyBlocksCancellationAfterDeadline()
{
    object? policy = CreateBookingPolicy(minimumAdvanceMinutes: 30, cancellationDeadlineHours: 12);

    if (policy is null)
    {
        return false;
    }

    System.Reflection.MethodInfo? ensureMethod = policy.GetType().GetMethod(
        "EnsureCancellationAllowed",
        [typeof(DateTimeOffset), typeof(DateTimeOffset)]);

    if (ensureMethod is null)
    {
        return false;
    }

    try
    {
        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        ensureMethod.Invoke(policy, [nowUtc.AddHours(6), nowUtc]);
        return false;
    }
    catch (System.Reflection.TargetInvocationException exception)
        when (exception.InnerException is InvalidOperationException)
    {
        return true;
    }
}

static object? CreateBookingPolicy(
    int minimumAdvanceMinutes,
    int cancellationDeadlineHours)
{
    Type? policyType = typeof(Booking).Assembly.GetType(
        "ReserveFlow.Modules.Bookings.Domain.BookingPolicies.BookingPolicy");
    System.Reflection.MethodInfo? configureMethod = policyType?.GetMethod(
        "Configure",
        [typeof(Guid), typeof(int), typeof(int)]);

    return configureMethod?.Invoke(null, [Guid.NewGuid(), minimumAdvanceMinutes, cancellationDeadlineHours]);
}

static bool OutboxMessageCapturesDomainEvent()
{
    var domainEvent = new BookingCreatedDomainEvent(
        Guid.NewGuid(),
        Guid.NewGuid(),
        DateTimeOffset.UtcNow,
        DateTimeOffset.UtcNow.AddHours(1));

    OutboxMessage message = OutboxMessage.FromDomainEvent(domainEvent);

    return message.TenantId == domainEvent.TenantId &&
           message.Type.EndsWith(nameof(BookingCreatedDomainEvent), StringComparison.Ordinal) &&
           message.Payload.Contains(nameof(BookingCreatedDomainEvent.BookingId), StringComparison.Ordinal);
}

static bool OutboxMessageRecordsProcessingOutcome()
{
    var domainEvent = new BookingCancelledDomainEvent(
        Guid.NewGuid(),
        Guid.NewGuid(),
        DateTimeOffset.UtcNow);

    OutboxMessage message = OutboxMessage.FromDomainEvent(domainEvent);
    message.RecordFailure("temporary failure");
    bool failed = message.RetryCount == 1 && message.Error == "temporary failure" && message.ProcessedOnUtc is null;

    DateTime processedOnUtc = DateTime.UtcNow;
    message.MarkProcessed(processedOnUtc);

    return failed && message.RetryCount == 1 && message.Error is null && message.ProcessedOnUtc == processedOnUtc;
}

static bool BookingCancelChangesStatusAndRaisesDomainEvent()
{
    Booking booking = Booking.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        staffMemberId: Guid.NewGuid(),
        resourceId: null,
        DateTimeOffset.UtcNow.AddHours(1),
        DateTimeOffset.UtcNow.AddHours(2));

    booking.ClearDomainEvents();
    booking.Cancel(DateTimeOffset.UtcNow);

    return booking.Status == BookingStatus.Cancelled &&
           booking.DomainEvents.OfType<BookingCancelledDomainEvent>().Any();
}

static bool BookingRescheduleChangesTimeAndRaisesDomainEvent()
{
    Booking booking = Booking.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        staffMemberId: Guid.NewGuid(),
        resourceId: null,
        DateTimeOffset.UtcNow.AddHours(1),
        DateTimeOffset.UtcNow.AddHours(2));

    DateTimeOffset newStart = DateTimeOffset.UtcNow.AddHours(3);
    DateTimeOffset newEnd = DateTimeOffset.UtcNow.AddHours(4);

    booking.ClearDomainEvents();
    booking.Reschedule(newStart, newEnd);

    return booking.Status == BookingStatus.Rescheduled &&
           booking.StartsAtUtc == newStart &&
           booking.EndsAtUtc == newEnd &&
           booking.DomainEvents.OfType<BookingRescheduledDomainEvent>().Any();
}

static bool BookingConfirmChangesStatusAndRaisesDomainEvent()
{
    Booking booking = Booking.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        staffMemberId: Guid.NewGuid(),
        resourceId: null,
        DateTimeOffset.UtcNow.AddHours(1),
        DateTimeOffset.UtcNow.AddHours(2));

    System.Reflection.MethodInfo? confirmMethod = typeof(Booking).GetMethod("Confirm", [typeof(DateTimeOffset)]);
    Type? confirmedEventType = typeof(Booking).Assembly.GetType(
        "ReserveFlow.Modules.Bookings.Domain.Bookings.BookingConfirmedDomainEvent");

    if (confirmMethod is null || confirmedEventType is null)
    {
        return false;
    }

    booking.ClearDomainEvents();
    confirmMethod.Invoke(booking, [DateTimeOffset.UtcNow]);

    return booking.Status == BookingStatus.Confirmed &&
           booking.DomainEvents.Any(confirmedEventType.IsInstanceOfType);
}

static bool BookingExpireChangesStatusAndRaisesDomainEvent()
{
    Booking booking = Booking.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        staffMemberId: Guid.NewGuid(),
        resourceId: null,
        DateTimeOffset.UtcNow.AddHours(1),
        DateTimeOffset.UtcNow.AddHours(2));

    System.Reflection.MethodInfo? expireMethod = typeof(Booking).GetMethod("Expire", [typeof(DateTimeOffset)]);
    Type? expiredEventType = typeof(Booking).Assembly.GetType(
        "ReserveFlow.Modules.Bookings.Domain.Bookings.BookingExpiredDomainEvent");

    if (expireMethod is null || expiredEventType is null)
    {
        return false;
    }

    booking.ClearDomainEvents();
    expireMethod.Invoke(booking, [DateTimeOffset.UtcNow]);

    return booking.Status == BookingStatus.Expired &&
           booking.DomainEvents.Any(expiredEventType.IsInstanceOfType);
}

static bool BookingCompleteChangesStatusAndRaisesDomainEvent()
{
    Booking booking = Booking.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        staffMemberId: Guid.NewGuid(),
        resourceId: null,
        DateTimeOffset.UtcNow.AddHours(-2),
        DateTimeOffset.UtcNow.AddHours(-1));

    booking.ClearDomainEvents();
    booking.Complete(DateTimeOffset.UtcNow);

    return booking.Status == BookingStatus.Completed &&
           booking.DomainEvents.OfType<BookingCompletedDomainEvent>().Any();
}

static bool BookingNoShowChangesStatusAndRaisesDomainEvent()
{
    Booking booking = Booking.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        staffMemberId: Guid.NewGuid(),
        resourceId: null,
        DateTimeOffset.UtcNow.AddHours(-2),
        DateTimeOffset.UtcNow.AddHours(-1));

    booking.ClearDomainEvents();
    booking.MarkAsNoShow(DateTimeOffset.UtcNow);

    return booking.Status == BookingStatus.NoShow &&
           booking.DomainEvents.OfType<BookingMarkedAsNoShowDomainEvent>().Any();
}

static bool TenantCreateNormalizesDataAndRaisesDomainEvent()
{
    Tenant tenant = Tenant.Create(
        "  Smile Dental Clinic  ",
        "smile-dental",
        "Asia/Tashkent");

    return tenant.Name == "Smile Dental Clinic" &&
           tenant.Slug == "smile-dental" &&
           tenant.Status == TenantStatus.Pending &&
           tenant.DomainEvents.OfType<TenantProvisionedDomainEvent>().Any();
}

static bool TenantCreateCanAssignCategory()
{
    Guid categoryId = Guid.NewGuid();

    Tenant tenant = Tenant.Create(
        "Smile Dental Clinic",
        "smile-dental",
        "Asia/Tashkent",
        categoryId);

    return tenant.CategoryId == categoryId;
}

static bool TenantCategoryCreateNormalizesDataAndRaisesDomainEvent()
{
    TenantCategory category = TenantCategory.Create(
        "  Dental Clinic  ",
        " DENTAL-CLINIC ",
        sortOrder: 20);

    return category.Name == "Dental Clinic" &&
           category.Slug == "dental-clinic" &&
           category.SortOrder == 20 &&
           category.IsActive &&
           category.DomainEvents.OfType<TenantCategoryCreatedDomainEvent>().Any();
}

static bool TenantActivateChangesStatusAndRaisesDomainEvent()
{
    Tenant tenant = Tenant.Create(
        "Smile Dental Clinic",
        "smile-dental",
        "Asia/Tashkent");

    DateTimeOffset activatedAtUtc = DateTimeOffset.UtcNow;

    tenant.ClearDomainEvents();
    tenant.Activate(activatedAtUtc);

    return tenant.Status == TenantStatus.Active &&
           tenant.DomainEvents.OfType<TenantActivatedDomainEvent>().Any(domainEvent =>
               domainEvent.ActivatedAtUtc == activatedAtUtc);
}

static bool TenantSuspendChangesStatusAndRaisesDomainEvent()
{
    Tenant tenant = Tenant.Create(
        "Smile Dental Clinic",
        "smile-dental",
        "Asia/Tashkent");

    DateTimeOffset suspendedAtUtc = DateTimeOffset.UtcNow;

    tenant.Activate(DateTimeOffset.UtcNow);
    tenant.ClearDomainEvents();
    tenant.Suspend(suspendedAtUtc);

    return tenant.Status == TenantStatus.Suspended &&
           tenant.DomainEvents.OfType<TenantSuspendedDomainEvent>().Any(domainEvent =>
               domainEvent.SuspendedAtUtc == suspendedAtUtc);
}

static bool ServiceCreateNormalizesDataAndRaisesDomainEvent()
{
    Service service = Service.Create(
        Guid.NewGuid(),
        "  Dental Consultation  ",
        durationMinutes: 45,
        price: 200000m,
        currency: " uzs ");

    return service.Name == "Dental Consultation" &&
           service.DurationMinutes == 45 &&
           service.Currency == "UZS" &&
           service.IsActive &&
           service.DomainEvents.OfType<ServiceCreatedDomainEvent>().Any();
}

static bool ServiceUpdateChangesDetailsAndRaisesDomainEvent()
{
    Service service = Service.Create(
        Guid.NewGuid(),
        "Dental Consultation",
        durationMinutes: 45,
        price: 200000m,
        currency: "UZS");

    service.ClearDomainEvents();
    service.Update(
        "  Teeth Cleaning  ",
        durationMinutes: 30,
        price: 150000m,
        currency: " uzs ");

    return service.Name == "Teeth Cleaning" &&
           service.DurationMinutes == 30 &&
           service.Price == 150000m &&
           service.Currency == "UZS" &&
           service.DomainEvents.OfType<ServiceUpdatedDomainEvent>().Any();
}

static bool ServiceDeactivateChangesActiveFlagAndRaisesDomainEvent()
{
    Service service = Service.Create(
        Guid.NewGuid(),
        "Dental Consultation",
        durationMinutes: 45,
        price: 200000m,
        currency: "UZS");

    service.ClearDomainEvents();
    service.Deactivate();

    return !service.IsActive &&
           service.DomainEvents.OfType<ServiceDeactivatedDomainEvent>().Any();
}

static bool StaffMemberCreateNormalizesDataAndRaisesDomainEvent()
{
    StaffMember staffMember = StaffMember.Create(
        Guid.NewGuid(),
        "  Dr. Ali  ",
        " ALI@SMILE.EXAMPLE ");

    return staffMember.DisplayName == "Dr. Ali" &&
           staffMember.Email == "ali@smile.example" &&
           staffMember.IsActive &&
           staffMember.DomainEvents.OfType<StaffMemberCreatedDomainEvent>().Any();
}

static bool StaffMemberUpdateChangesDetailsAndRaisesDomainEvent()
{
    StaffMember staffMember = StaffMember.Create(
        Guid.NewGuid(),
        "Dr. Ali",
        "ali@smile.example");

    staffMember.ClearDomainEvents();
    staffMember.Update(
        "  Dr. Madina  ",
        " MADINA@SMILE.EXAMPLE ");

    return staffMember.DisplayName == "Dr. Madina" &&
           staffMember.Email == "madina@smile.example" &&
           staffMember.DomainEvents.OfType<StaffMemberUpdatedDomainEvent>().Any();
}

static bool StaffMemberDeactivateChangesActiveFlagAndRaisesDomainEvent()
{
    StaffMember staffMember = StaffMember.Create(
        Guid.NewGuid(),
        "Dr. Ali",
        "ali@smile.example");

    staffMember.ClearDomainEvents();
    staffMember.Deactivate();

    return !staffMember.IsActive &&
           staffMember.DomainEvents.OfType<StaffMemberDeactivatedDomainEvent>().Any();
}

static bool ResourceCreateNormalizesDataAndRaisesDomainEvent()
{
    Resource resource = Resource.Create(
        Guid.NewGuid(),
        "  Room 2  ",
        " treatment-room ",
        capacity: 1);

    return resource.Name == "Room 2" &&
           resource.ResourceType == "treatment-room" &&
           resource.Capacity == 1 &&
           resource.IsActive &&
           resource.DomainEvents.OfType<ResourceCreatedDomainEvent>().Any();
}

static bool ResourceUpdateChangesDetailsAndRaisesDomainEvent()
{
    Resource resource = Resource.Create(
        Guid.NewGuid(),
        "Room 2",
        "treatment-room",
        capacity: 1);

    resource.ClearDomainEvents();
    resource.Update(
        "  Room 3  ",
        " X-Ray-Room ",
        capacity: 2);

    return resource.Name == "Room 3" &&
           resource.ResourceType == "x-ray-room" &&
           resource.Capacity == 2 &&
           resource.DomainEvents.OfType<ResourceUpdatedDomainEvent>().Any();
}

static bool ResourceDeactivateChangesActiveFlagAndRaisesDomainEvent()
{
    Resource resource = Resource.Create(
        Guid.NewGuid(),
        "Room 2",
        "treatment-room",
        capacity: 1);

    resource.ClearDomainEvents();
    resource.Deactivate();

    return !resource.IsActive &&
           resource.DomainEvents.OfType<ResourceDeactivatedDomainEvent>().Any();
}

static bool WorkingHourCreateTargetsStaffOrResourceAndRaisesDomainEvent()
{
    Guid tenantId = Guid.NewGuid();
    Guid staffMemberId = Guid.NewGuid();

    WorkingHour workingHour = WorkingHour.Create(
        tenantId,
        staffMemberId,
        resourceId: null,
        DayOfWeek.Monday,
        new TimeOnly(9, 0),
        new TimeOnly(18, 0));

    return workingHour.TenantId == tenantId &&
           workingHour.StaffMemberId == staffMemberId &&
           workingHour.ResourceId is null &&
           workingHour.DomainEvents.OfType<WorkingHourCreatedDomainEvent>().Any();
}

static bool UnavailablePeriodCreateTargetsStaffOrResourceAndRaisesDomainEvent()
{
    Guid tenantId = Guid.NewGuid();
    Guid resourceId = Guid.NewGuid();
    DateTimeOffset startsAtUtc = DateTimeOffset.UtcNow.AddHours(1);
    DateTimeOffset endsAtUtc = startsAtUtc.AddHours(2);

    UnavailablePeriod unavailablePeriod = UnavailablePeriod.Create(
        tenantId,
        staffMemberId: null,
        resourceId,
        startsAtUtc,
        endsAtUtc,
        "  Maintenance  ");

    return unavailablePeriod.TenantId == tenantId &&
           unavailablePeriod.ResourceId == resourceId &&
           unavailablePeriod.Reason == "Maintenance" &&
           unavailablePeriod.DomainEvents.OfType<UnavailablePeriodCreatedDomainEvent>().Any();
}

static bool NotificationQueueNormalizesDataAndRaisesDomainEvent()
{
    NotificationMessage message = NotificationMessage.Queue(
        Guid.NewGuid(),
        NotificationChannel.Email,
        "  CUSTOMER@EXAMPLE.COM  ",
        "  Booking confirmed  ",
        "  Your booking is confirmed.  ");

    return message.Recipient == "customer@example.com" &&
           message.Subject == "Booking confirmed" &&
           message.Body == "Your booking is confirmed." &&
           message.Status == NotificationStatus.Pending &&
           message.DomainEvents.OfType<NotificationQueuedDomainEvent>().Any();
}

static bool AuditLogNormalizesDataAndRaisesDomainEvent()
{
    AuditLog log = AuditLog.Record(
        tenantId: Guid.NewGuid(),
        userId: Guid.NewGuid(),
        action: "  Booking.Cancelled  ",
        entityName: "  Booking  ",
        entityId: Guid.NewGuid(),
        detailsJson: " {\"reason\":\"customer-request\"} ");

    return log.Action == "Booking.Cancelled" &&
           log.EntityName == "Booking" &&
           log.DetailsJson == "{\"reason\":\"customer-request\"}" &&
           log.DomainEvents.OfType<AuditLogRecordedDomainEvent>().Any();
}

static bool DailyBookingReportValidatesCountsAndRaisesDomainEvent()
{
    DailyBookingReport report = DailyBookingReport.Record(
        tenantId: Guid.NewGuid(),
        date: new DateOnly(2026, 6, 1),
        createdBookings: 12,
        cancelledBookings: 2,
        completedBookings: 8,
        noShowBookings: 1);

    return report.Date == new DateOnly(2026, 6, 1) &&
           report.CreatedBookings == 12 &&
           report.CancelledBookings == 2 &&
           report.CompletedBookings == 8 &&
           report.NoShowBookings == 1 &&
           report.DomainEvents.OfType<DailyBookingReportRecordedDomainEvent>().Any();
}

static bool WebhookInboxMessageNormalizesDataAndRaisesDomainEvent()
{
    WebhookInboxMessage message = WebhookInboxMessage.Accept(
        tenantId: Guid.NewGuid(),
        source: "  GoogleCalendar  ",
        externalMessageId: "  evt-123  ",
        eventType: "  CalendarEventChanged  ",
        payloadJson: " {\"id\":\"evt-123\"} ");

    return message.Source == "googlecalendar" &&
           message.ExternalMessageId == "evt-123" &&
           message.EventType == "CalendarEventChanged" &&
           message.PayloadJson == "{\"id\":\"evt-123\"}" &&
           message.Status == WebhookInboxStatus.Received &&
           message.DomainEvents.OfType<WebhookInboxMessageReceivedDomainEvent>().Any();
}

static bool KeycloakRoleParserAcceptsRealmAccessRoles()
{
    var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [new Claim("realm_access", "{\"roles\":[\"tenant-admin\"]}")],
        authenticationType: "jwt"));

    return KeycloakRoleClaims.HasAnyRole(principal, KeycloakRoles.TenantAdmin);
}

static bool KeycloakRoleParserAcceptsResourceAccessRoles()
{
    var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [new Claim("resource_access", "{\"reserveflow-api\":{\"roles\":[\"staff\"]}}")],
        authenticationType: "jwt"));

    return KeycloakRoleClaims.HasAnyRole(principal, KeycloakRoles.Staff);
}

static bool KeycloakRoleParserAcceptsSimpleRoleClaims()
{
    var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [new Claim("roles", "PlatformAdmin")],
        authenticationType: "jwt"));

    return KeycloakRoleClaims.HasAnyRole(principal, KeycloakRoles.PlatformAdmin);
}

static bool HttpCurrentUserReadsKeycloakClaims()
{
    Guid userId = Guid.NewGuid();
    var httpContext = new DefaultHttpContext
    {
        User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("sub", "keycloak-subject"),
                new Claim("user_id", userId.ToString()),
                new Claim("email", "ADMIN@SMILE.EXAMPLE"),
                new Claim("permissions", "Bookings.Create Reports.View")
            ],
            authenticationType: "jwt"))
    };

    var currentUser = new HttpCurrentUser(new HttpContextAccessor { HttpContext = httpContext });

    return currentUser.UserId == userId &&
           currentUser.KeycloakSubject == "keycloak-subject" &&
           currentUser.Email == "admin@smile.example" &&
           currentUser.Permissions.Contains("Bookings.Create") &&
           currentUser.Permissions.Contains("Reports.View");
}

static bool HttpTenantContextReadsTenantHeadersBeforeClaims()
{
    Guid headerTenantId = Guid.NewGuid();
    Guid claimTenantId = Guid.NewGuid();
    var httpContext = new DefaultHttpContext
    {
        User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("tenant_id", claimTenantId.ToString()),
                new Claim("tenant_slug", "claim-tenant"),
                new Claim("tenant_timezone", "UTC")
            ],
            authenticationType: "jwt"))
    };

    httpContext.Request.Headers["X-Tenant-Id"] = new StringValues(headerTenantId.ToString());
    httpContext.Request.Headers["X-Tenant-Slug"] = new StringValues("header-tenant");
    httpContext.Request.Headers["X-Tenant-TimeZone"] = new StringValues("Asia/Tashkent");

    var tenantContext = new HttpTenantContext(new HttpContextAccessor { HttpContext = httpContext });

    return tenantContext.TenantId == headerTenantId &&
           tenantContext.TenantSlug == "header-tenant" &&
           tenantContext.TimeZoneId == "Asia/Tashkent" &&
           !tenantContext.IsPlatformScope;
}

static bool TenantAccessGuardAllowsCurrentTenant()
{
    Guid tenantId = Guid.NewGuid();
    var guard = new TenantAccessGuard(new TestTenantContext(tenantId, isPlatformScope: false));

    return guard.CanAccessTenant(tenantId);
}

static bool TenantAccessGuardAllowsPlatformScope()
{
    Guid tenantId = Guid.NewGuid();
    var guard = new TenantAccessGuard(new TestTenantContext(tenantId: null, isPlatformScope: true));

    return guard.CanAccessTenant(tenantId);
}

static bool TenantAccessGuardDeniesDifferentTenant()
{
    var guard = new TenantAccessGuard(new TestTenantContext(Guid.NewGuid(), isPlatformScope: false));

    return !guard.CanAccessTenant(Guid.NewGuid());
}

static bool TenantAdminTenantIdEndpointsRequireTenantAccess()
{
    string[] endpointFiles =
    [
        "src/Modules/Catalog/ReserveFlow.Modules.Catalog.Presentation/CreateServiceEndpoint.cs",
        "src/Modules/Catalog/ReserveFlow.Modules.Catalog.Presentation/GetAdminServicesEndpoint.cs",
        "src/Modules/Staffing/ReserveFlow.Modules.Staffing.Presentation/CreateStaffMemberEndpoint.cs",
        "src/Modules/Staffing/ReserveFlow.Modules.Staffing.Presentation/GetAdminStaffMembersEndpoint.cs",
        "src/Modules/Resources/ReserveFlow.Modules.Resources.Presentation/CreateResourceEndpoint.cs",
        "src/Modules/Resources/ReserveFlow.Modules.Resources.Presentation/GetAdminResourcesEndpoint.cs",
        "src/Modules/Scheduling/ReserveFlow.Modules.Scheduling.Presentation/CreateStaffWorkingHourEndpoint.cs",
        "src/Modules/Scheduling/ReserveFlow.Modules.Scheduling.Presentation/CreateResourceWorkingHourEndpoint.cs",
        "src/Modules/Scheduling/ReserveFlow.Modules.Scheduling.Presentation/CreateStaffUnavailablePeriodEndpoint.cs",
        "src/Modules/Scheduling/ReserveFlow.Modules.Scheduling.Presentation/CreateResourceUnavailablePeriodEndpoint.cs",
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/ConfigureBookingPolicyEndpoint.cs",
        "src/Modules/Notifications/ReserveFlow.Modules.Notifications.Presentation/QueueNotificationEndpoint.cs",
        "src/Modules/Audit/ReserveFlow.Modules.Audit.Presentation/RecordAuditLogEndpoint.cs",
        "src/Modules/Reporting/ReserveFlow.Modules.Reporting.Presentation/RecordDailyBookingReportEndpoint.cs",
        "src/Modules/Reporting/ReserveFlow.Modules.Reporting.Presentation/GetDailyBookingReportEndpoint.cs"
    ];

    return endpointFiles.All(file => SourceContains(file, "RequireTenantAccess()"));
}

static bool CatalogIdOnlyAdminHandlersEnforceTenantOwnership()
{
    string[] handlerFiles =
    [
        "src/Modules/Catalog/ReserveFlow.Modules.Catalog.Application/Services/GetService/GetServiceQueryHandler.cs",
        "src/Modules/Catalog/ReserveFlow.Modules.Catalog.Application/Services/UpdateService/UpdateServiceCommandHandler.cs",
        "src/Modules/Catalog/ReserveFlow.Modules.Catalog.Application/Services/DeactivateService/DeactivateServiceCommandHandler.cs"
    ];

    return handlerFiles.All(HandlerSourceHasTenantAccessGuard);
}

static bool StaffingIdOnlyAdminHandlersEnforceTenantOwnership()
{
    string[] handlerFiles =
    [
        "src/Modules/Staffing/ReserveFlow.Modules.Staffing.Application/StaffMembers/GetStaffMember/GetStaffMemberQueryHandler.cs",
        "src/Modules/Staffing/ReserveFlow.Modules.Staffing.Application/StaffMembers/UpdateStaffMember/UpdateStaffMemberCommandHandler.cs",
        "src/Modules/Staffing/ReserveFlow.Modules.Staffing.Application/StaffMembers/DeactivateStaffMember/DeactivateStaffMemberCommandHandler.cs"
    ];

    return handlerFiles.All(HandlerSourceHasTenantAccessGuard);
}

static bool ResourcesIdOnlyAdminHandlersEnforceTenantOwnership()
{
    string[] handlerFiles =
    [
        "src/Modules/Resources/ReserveFlow.Modules.Resources.Application/Resources/GetResource/GetResourceQueryHandler.cs",
        "src/Modules/Resources/ReserveFlow.Modules.Resources.Application/Resources/UpdateResource/UpdateResourceCommandHandler.cs",
        "src/Modules/Resources/ReserveFlow.Modules.Resources.Application/Resources/DeactivateResource/DeactivateResourceCommandHandler.cs"
    ];

    return handlerFiles.All(HandlerSourceHasTenantAccessGuard);
}

static bool BookingIdOnlyHandlersEnforceTenantOwnership()
{
    string[] handlerFiles =
    [
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Application/Bookings/CancelBooking/CancelBookingCommandHandler.cs",
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Application/Bookings/RescheduleBooking/RescheduleBookingCommandHandler.cs",
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Application/Bookings/ConfirmBooking/ConfirmBookingCommandHandler.cs",
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Application/Bookings/ExpirePendingBooking/ExpirePendingBookingCommandHandler.cs",
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Application/Bookings/CompleteBooking/CompleteBookingCommandHandler.cs",
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Application/Bookings/MarkBookingAsNoShow/MarkBookingAsNoShowCommandHandler.cs"
    ];

    return handlerFiles.All(HandlerSourceHasTenantAccessGuard);
}

static bool PublicCancelEndpointUsesPublicLookupCredentials()
{
    string endpointFile = "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/PublicCancelBookingEndpoint.cs";

    return SourceContains(endpointFile, "/api/public/tenants/{tenantSlug}/bookings/{publicReference}/cancel") &&
           SourceContains(endpointFile, "PublicCancelBookingRequest") &&
           SourceContains(endpointFile, "CancelPublicBookingCommand") &&
           !SourceContains(endpointFile, "{bookingId:guid}");
}

static bool AdminBookingHandlersUseTenantScopedRepositoryLookups()
{
    string listHandlerFile = "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Application/Bookings/GetBookings/GetBookingsQueryHandler.cs";
    string detailHandlerFile = "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Application/Bookings/GetBooking/GetBookingQueryHandler.cs";

    return SourceContains(listHandlerFile, "GetByTenantIdAsync") &&
           SourceContains(detailHandlerFile, "GetByTenantIdAndIdAsync");
}

static bool PublicCancelHandlerUsesPublicLookupAndPolicy()
{
    string handlerFile = "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Application/Bookings/CancelPublicBooking/CancelPublicBookingCommandHandler.cs";

    return SourceContains(handlerFile, "FindByPublicLookupAsync") &&
           SourceContains(handlerFile, "EnsureCancellationAllowed") &&
           SourceContains(handlerFile, "BookingHistoryEntry.Record");
}

static bool PublicBookingRepositoryLookupIsTenantScoped()
{
    string repositoryFile = "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Infrastructure/Bookings/BookingRepository.cs";

    return SourceContains(repositoryFile, "FindByPublicLookupAsync(") &&
           SourceContains(repositoryFile, "Guid tenantId") &&
           SourceContains(repositoryFile, "tenantId == Guid.Empty") &&
           SourceContains(repositoryFile, "booking.TenantId == tenantId") &&
           SourceContains(repositoryFile, "booking.PublicReference == normalizedPublicReference");
}

static bool PublicRescheduleHandlerUsesPublicLookupPolicyAvailabilityAndHistory()
{
    string handlerFile = "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Application/Bookings/PublicRescheduleBooking/PublicRescheduleBookingCommandHandler.cs";

    return SourceContains(handlerFile, "FindByPublicLookupAsync") &&
           SourceContains(handlerFile, "IBookingPolicyRepository") &&
           SourceContains(handlerFile, "EnsureBookingCanStartAt") &&
           SourceContains(handlerFile, "IBookingAvailabilityChecker") &&
           SourceContains(handlerFile, "HasOverlapAsync") &&
           SourceContains(handlerFile, "BookingHistoryEntry.Record") &&
           SourceContains(handlerFile, "\"Public booking rescheduled\"");
}

static bool PostgresDatabaseBootstrapperUsesMaintenanceConnection()
{
    string bootstrapperFile = "src/Common/ReserveFlow.Common.Infrastructure/Data/PostgresDatabaseBootstrapper.cs";

    return SourceContains(bootstrapperFile, "NpgsqlConnectionStringBuilder") &&
           SourceContains(bootstrapperFile, "maintenanceBuilder") &&
           SourceContains(bootstrapperFile, "maintenanceDatabase") &&
           SourceContains(bootstrapperFile, "create database") &&
           SourceContains(bootstrapperFile, "QuoteIdentifier");
}

static bool HandlerSourceHasTenantAccessGuard(string handlerFile)
{
    return SourceContains(handlerFile, "ITenantAccessGuard") &&
           SourceContains(handlerFile, "tenantAccessGuard.CanAccessTenant");
}

static bool AvailabilityEngineGeneratesFixedDurationSlots()
{
    var workingWindow = new AvailabilityWindow(
        new DateTimeOffset(2026, 6, 1, 9, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero));

    IReadOnlyList<AvailableSlot> slots = AvailabilityEngine.GenerateSlots(
        [workingWindow],
        serviceDuration: TimeSpan.FromMinutes(60),
        step: TimeSpan.FromMinutes(30));

    return slots.Count == 5 &&
           slots[0].StartsAtUtc == workingWindow.StartsAtUtc &&
           slots[^1].StartsAtUtc == new DateTimeOffset(2026, 6, 1, 11, 0, 0, TimeSpan.Zero);
}

sealed class TestTenantContext(Guid? tenantId, bool isPlatformScope) : ITenantContext
{
    public Guid? TenantId { get; } = tenantId;

    public string? TenantSlug => null;

    public string? TimeZoneId => null;

    public bool IsPlatformScope { get; } = isPlatformScope;
}
