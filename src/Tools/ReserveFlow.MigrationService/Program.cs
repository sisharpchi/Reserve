using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReserveFlow.Modules.Audit.Infrastructure;
using ReserveFlow.Modules.Audit.Infrastructure.Database;
using ReserveFlow.Modules.Bookings.Infrastructure;
using ReserveFlow.Modules.Bookings.Infrastructure.Database;
using ReserveFlow.Modules.Catalog.Infrastructure;
using ReserveFlow.Modules.Catalog.Infrastructure.Database;
using ReserveFlow.Modules.Identity.Infrastructure;
using ReserveFlow.Modules.Identity.Infrastructure.Database;
using ReserveFlow.Modules.Integrations.Infrastructure;
using ReserveFlow.Modules.Integrations.Infrastructure.Database;
using ReserveFlow.Modules.Notifications.Infrastructure;
using ReserveFlow.Modules.Notifications.Infrastructure.Database;
using ReserveFlow.Modules.Reporting.Infrastructure;
using ReserveFlow.Modules.Reporting.Infrastructure.Database;
using ReserveFlow.Modules.Resources.Infrastructure;
using ReserveFlow.Modules.Resources.Infrastructure.Database;
using ReserveFlow.Modules.Scheduling.Infrastructure;
using ReserveFlow.Modules.Scheduling.Infrastructure.Database;
using ReserveFlow.Modules.Staffing.Infrastructure;
using ReserveFlow.Modules.Staffing.Infrastructure.Database;
using ReserveFlow.Modules.Tenants.Infrastructure;
using ReserveFlow.Modules.Tenants.Infrastructure.Database;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
});

builder.Configuration
    .AddJsonFile(
        Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
        optional: true,
        reloadOnChange: false)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

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

using IHost host = builder.Build();
using IServiceScope scope = host.Services.CreateScope();
using var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};

try
{
    await MigrateModuleAsync<IdentityDbContext>(scope, "Identity", cancellationTokenSource.Token);
    await MigrateModuleAsync<TenantsDbContext>(scope, "Tenants", cancellationTokenSource.Token);
    await MigrateModuleAsync<CatalogDbContext>(scope, "Catalog", cancellationTokenSource.Token);
    await MigrateModuleAsync<StaffingDbContext>(scope, "Staffing", cancellationTokenSource.Token);
    await MigrateModuleAsync<ResourcesDbContext>(scope, "Resources", cancellationTokenSource.Token);
    await MigrateModuleAsync<SchedulingDbContext>(scope, "Scheduling", cancellationTokenSource.Token);
    await MigrateModuleAsync<BookingsDbContext>(scope, "Bookings", cancellationTokenSource.Token);
    await MigrateModuleAsync<NotificationsDbContext>(scope, "Notifications", cancellationTokenSource.Token);
    await MigrateModuleAsync<AuditDbContext>(scope, "Audit", cancellationTokenSource.Token);
    await MigrateModuleAsync<ReportingDbContext>(scope, "Reporting", cancellationTokenSource.Token);
    await MigrateModuleAsync<IntegrationsDbContext>(scope, "Integrations", cancellationTokenSource.Token);

    Console.WriteLine("ReserveFlow database migrations completed.");
    return 0;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("ReserveFlow database migrations were cancelled.");
    return 2;
}
catch (Exception exception)
{
    Console.Error.WriteLine("ReserveFlow database migrations failed.");
    Console.Error.WriteLine(exception);
    return 1;
}

static async Task MigrateModuleAsync<TDbContext>(
    IServiceScope scope,
    string moduleName,
    CancellationToken cancellationToken)
    where TDbContext : DbContext
{
    Console.WriteLine($"Applying {moduleName} migrations...");

    TDbContext dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
    await dbContext.Database.MigrateAsync(cancellationToken);

    Console.WriteLine($"Applied {moduleName} migrations.");
}
