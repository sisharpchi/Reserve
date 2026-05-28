using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using ReserveFlow.Api.Extensions;
using ReserveFlow.Common.Infrastructure;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Audit.Infrastructure;
using ReserveFlow.Modules.Bookings.Infrastructure;
using ReserveFlow.Modules.Catalog.Infrastructure;
using ReserveFlow.Modules.Identity.Infrastructure;
using ReserveFlow.Modules.Integrations.Infrastructure;
using ReserveFlow.Modules.Notifications.Infrastructure;
using ReserveFlow.Modules.Reporting.Infrastructure;
using ReserveFlow.Modules.Resources.Infrastructure;
using ReserveFlow.Modules.Scheduling.Infrastructure;
using ReserveFlow.Modules.Staffing.Infrastructure;
using ReserveFlow.Modules.Tenants.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

builder.Configuration.AddModuleConfiguration(["identity", "tenants", "catalog", "staffing", "resources", "scheduling", "bookings", "notifications", "audit", "reporting", "integrations"]);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddReserveFlowInfrastructure(builder.Configuration);

builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddTenantsModule(builder.Configuration);
builder.Services.AddCatalogModule(builder.Configuration);
builder.Services.AddStaffingModule(builder.Configuration);
builder.Services.AddResourcesModule(builder.Configuration);
builder.Services.AddSchedulingModule(builder.Configuration);
builder.Services.AddBookingsModule(builder.Configuration);
builder.Services.AddNotificationsModule(builder.Configuration);
builder.Services.AddAuditModule(builder.Configuration);
builder.Services.AddReportingModule(builder.Configuration);
builder.Services.AddIntegrationsModule(builder.Configuration);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseCors(CorsPolicies.WebApp);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("ready")
});

app.MapGet("/", () => Results.Ok(new
{
    Application = "ReserveFlow",
    Runtime = ".NET 8",
    Architecture = "Modular Monolith",
    IdentityProvider = "Keycloak"
}))
.WithTags("System");

app.MapEndpoints();

app.Run();

public partial class Program;
