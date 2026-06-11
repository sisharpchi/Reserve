using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Audit.Infrastructure.Database;
using ReserveFlow.Modules.Bookings.Domain.BookingPolicies;
using ReserveFlow.Modules.Bookings.Infrastructure.Database;
using ReserveFlow.Modules.Catalog.Domain.Services;
using ReserveFlow.Modules.Catalog.Infrastructure.Database;
using ReserveFlow.Modules.Identity.Infrastructure.Database;
using ReserveFlow.Modules.Integrations.Infrastructure.Database;
using ReserveFlow.Modules.Notifications.Infrastructure.Database;
using ReserveFlow.Modules.Reporting.Infrastructure.Database;
using ReserveFlow.Modules.Resources.Infrastructure.Database;
using ReserveFlow.Modules.Scheduling.Domain.WorkingHours;
using ReserveFlow.Modules.Scheduling.Infrastructure.Database;
using ReserveFlow.Modules.Staffing.Domain.StaffMembers;
using ReserveFlow.Modules.Staffing.Infrastructure.Database;
using ReserveFlow.Modules.Tenants.Domain.TenantCategories;
using ReserveFlow.Modules.Tenants.Domain.Tenants;
using ReserveFlow.Modules.Tenants.Infrastructure.Database;
using ResourceEntity = ReserveFlow.Modules.Resources.Domain.Resources.Resource;

namespace ReserveFlow.Api.DemoData;

internal sealed class DemoDataSeederHostedService(
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration,
    ILogger<DemoDataSeederHostedService> logger) : IHostedService
{
    private const string SeedDemoTenantConfigurationKey = "Tenants:SeedDemoTenant";
    private const string DemoCategorySlug = "clinic";
    private const string DemoCategoryName = "Clinic";
    private const string DemoTenantSlug = "smile-dental";
    private const string DemoTenantName = "Smile Dental Clinic";
    private const string DemoTenantTimeZone = "Asia/Tashkent";
    private const string DentalConsultationServiceName = "Dental Consultation";
    private const string TeethCleaningServiceName = "Teeth Cleaning";
    private const string DrAliDisplayName = "Dr. Ali Karimov";
    private const string DrAliEmail = "dr.ali@smile-dental.example";
    private const string RoomOneName = "Room 1";
    private const string ResourceTypeRoom = "room";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!configuration.GetValue(SeedDemoTenantConfigurationKey, defaultValue: false))
        {
            LogDemoSeederDisabled(logger, null);
            return;
        }

        using IServiceScope scope = serviceScopeFactory.CreateScope();

        IdentityDbContext identityDbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        TenantsDbContext tenantsDbContext = scope.ServiceProvider.GetRequiredService<TenantsDbContext>();
        CatalogDbContext catalogDbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        StaffingDbContext staffingDbContext = scope.ServiceProvider.GetRequiredService<StaffingDbContext>();
        ResourcesDbContext resourcesDbContext = scope.ServiceProvider.GetRequiredService<ResourcesDbContext>();
        SchedulingDbContext schedulingDbContext = scope.ServiceProvider.GetRequiredService<SchedulingDbContext>();
        BookingsDbContext bookingsDbContext = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
        NotificationsDbContext notificationsDbContext = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        AuditDbContext auditDbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        ReportingDbContext reportingDbContext = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
        IntegrationsDbContext integrationsDbContext = scope.ServiceProvider.GetRequiredService<IntegrationsDbContext>();

        await ApplyMigrationsAsync(
            identityDbContext,
            tenantsDbContext,
            catalogDbContext,
            staffingDbContext,
            resourcesDbContext,
            schedulingDbContext,
            bookingsDbContext,
            notificationsDbContext,
            auditDbContext,
            reportingDbContext,
            integrationsDbContext,
            cancellationToken);

        TenantCategory category = await EnsureDemoCategoryAsync(tenantsDbContext, cancellationToken);
        Tenant tenant = await EnsureDemoTenantAsync(tenantsDbContext, category.Id, cancellationToken);

        await EnsureDemoServicesAsync(catalogDbContext, tenant.Id, cancellationToken);

        Guid staffMemberId = await EnsureDemoStaffAsync(staffingDbContext, tenant.Id, cancellationToken);
        Guid resourceId = await EnsureDemoResourceAsync(resourcesDbContext, tenant.Id, cancellationToken);

        await EnsureDemoWorkingHoursAsync(schedulingDbContext, tenant.Id, staffMemberId, resourceId, cancellationToken);
        await EnsureDemoBookingPolicyAsync(bookingsDbContext, tenant.Id, cancellationToken);

        LogDemoSeedCompleted(logger, tenant.Id, DemoTenantSlug, null);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static async Task ApplyMigrationsAsync(
        IdentityDbContext identityDbContext,
        TenantsDbContext tenantsDbContext,
        CatalogDbContext catalogDbContext,
        StaffingDbContext staffingDbContext,
        ResourcesDbContext resourcesDbContext,
        SchedulingDbContext schedulingDbContext,
        BookingsDbContext bookingsDbContext,
        NotificationsDbContext notificationsDbContext,
        AuditDbContext auditDbContext,
        ReportingDbContext reportingDbContext,
        IntegrationsDbContext integrationsDbContext,
        CancellationToken cancellationToken)
    {
        await identityDbContext.Database.MigrateAsync(cancellationToken);
        await tenantsDbContext.Database.MigrateAsync(cancellationToken);
        await catalogDbContext.Database.MigrateAsync(cancellationToken);
        await staffingDbContext.Database.MigrateAsync(cancellationToken);
        await resourcesDbContext.Database.MigrateAsync(cancellationToken);
        await schedulingDbContext.Database.MigrateAsync(cancellationToken);
        await bookingsDbContext.Database.MigrateAsync(cancellationToken);
        await notificationsDbContext.Database.MigrateAsync(cancellationToken);
        await auditDbContext.Database.MigrateAsync(cancellationToken);
        await reportingDbContext.Database.MigrateAsync(cancellationToken);
        await integrationsDbContext.Database.MigrateAsync(cancellationToken);
    }

    private static async Task<TenantCategory> EnsureDemoCategoryAsync(
        TenantsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        TenantCategory? category = await dbContext.TenantCategories
            .SingleOrDefaultAsync(candidate => candidate.Slug == DemoCategorySlug, cancellationToken);

        if (category is not null)
        {
            return category;
        }

        category = TenantCategory.Create(DemoCategoryName, DemoCategorySlug, sortOrder: 10);
        category.ClearDomainEvents();

        dbContext.TenantCategories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return category;
    }

    private static async Task<Tenant> EnsureDemoTenantAsync(
        TenantsDbContext dbContext,
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        Tenant? tenant = await dbContext.Tenants
            .SingleOrDefaultAsync(candidate => candidate.Slug == DemoTenantSlug, cancellationToken);

        if (tenant is null)
        {
            tenant = Tenant.Create(DemoTenantName, DemoTenantSlug, DemoTenantTimeZone, categoryId);
            dbContext.Tenants.Add(tenant);
        }

        if (tenant.Status is not TenantStatus.Active)
        {
            tenant.Activate(DateTimeOffset.UtcNow);
        }

        tenant.ClearDomainEvents();
        await dbContext.SaveChangesAsync(cancellationToken);

        return tenant;
    }

    private static async Task EnsureDemoServicesAsync(
        CatalogDbContext dbContext,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Services.AnyAsync(
                service => service.TenantId == tenantId && service.Name == DentalConsultationServiceName,
                cancellationToken))
        {
            Service service = Service.Create(
                tenantId,
                DentalConsultationServiceName,
                durationMinutes: 30,
                price: 200_000m,
                currency: "UZS");

            service.ClearDomainEvents();
            dbContext.Services.Add(service);
        }

        if (!await dbContext.Services.AnyAsync(
                service => service.TenantId == tenantId && service.Name == TeethCleaningServiceName,
                cancellationToken))
        {
            Service service = Service.Create(
                tenantId,
                TeethCleaningServiceName,
                durationMinutes: 45,
                price: 350_000m,
                currency: "UZS");

            service.ClearDomainEvents();
            dbContext.Services.Add(service);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<Guid> EnsureDemoStaffAsync(
        StaffingDbContext dbContext,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        StaffMember? staffMember = await dbContext.StaffMembers
            .SingleOrDefaultAsync(
                candidate => candidate.TenantId == tenantId && candidate.Email == DrAliEmail,
                cancellationToken);

        if (staffMember is not null)
        {
            return staffMember.Id;
        }

        staffMember = StaffMember.Create(tenantId, DrAliDisplayName, DrAliEmail);
        staffMember.ClearDomainEvents();

        dbContext.StaffMembers.Add(staffMember);
        await dbContext.SaveChangesAsync(cancellationToken);

        return staffMember.Id;
    }

    private static async Task<Guid> EnsureDemoResourceAsync(
        ResourcesDbContext dbContext,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        ResourceEntity? resource = await dbContext.Resources
            .SingleOrDefaultAsync(
                candidate => candidate.TenantId == tenantId && candidate.Name == RoomOneName,
                cancellationToken);

        if (resource is not null)
        {
            return resource.Id;
        }

        resource = ResourceEntity.Create(
            tenantId,
            RoomOneName,
            ResourceTypeRoom,
            capacity: 1);

        resource.ClearDomainEvents();

        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync(cancellationToken);

        return resource.Id;
    }

    private static async Task EnsureDemoWorkingHoursAsync(
        SchedulingDbContext dbContext,
        Guid tenantId,
        Guid staffMemberId,
        Guid resourceId,
        CancellationToken cancellationToken)
    {
        foreach (DayOfWeek dayOfWeek in GetBusinessWeekDays())
        {
            await EnsureWorkingHourAsync(
                dbContext,
                tenantId,
                staffMemberId,
                resourceId: null,
                dayOfWeek,
                cancellationToken);

            await EnsureWorkingHourAsync(
                dbContext,
                tenantId,
                staffMemberId: null,
                resourceId,
                dayOfWeek,
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureWorkingHourAsync(
        SchedulingDbContext dbContext,
        Guid tenantId,
        Guid? staffMemberId,
        Guid? resourceId,
        DayOfWeek dayOfWeek,
        CancellationToken cancellationToken)
    {
        var startsAt = new TimeOnly(9, 0);
        var endsAt = new TimeOnly(18, 0);

        bool exists = await dbContext.WorkingHours.AnyAsync(
            workingHour =>
                workingHour.TenantId == tenantId &&
                workingHour.StaffMemberId == staffMemberId &&
                workingHour.ResourceId == resourceId &&
                workingHour.DayOfWeek == dayOfWeek &&
                workingHour.StartsAt == startsAt &&
                workingHour.EndsAt == endsAt,
            cancellationToken);

        if (exists)
        {
            return;
        }

        WorkingHour workingHour = WorkingHour.Create(
            tenantId,
            staffMemberId,
            resourceId,
            dayOfWeek,
            startsAt,
            endsAt);

        workingHour.ClearDomainEvents();
        dbContext.WorkingHours.Add(workingHour);
    }

    private static async Task EnsureDemoBookingPolicyAsync(
        BookingsDbContext dbContext,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        bool exists = await dbContext.BookingPolicies
            .AnyAsync(policy => policy.TenantId == tenantId, cancellationToken);

        if (exists)
        {
            return;
        }

        BookingPolicy policy = BookingPolicy.Configure(
            tenantId,
            minimumAdvanceMinutes: 60,
            cancellationDeadlineHours: 12);

        policy.ClearDomainEvents();

        dbContext.BookingPolicies.Add(policy);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static DayOfWeek[] GetBusinessWeekDays()
    {
        return
        [
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday
        ];
    }

    private static readonly Action<ILogger, Exception?> LogDemoSeederDisabled =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1, nameof(LogDemoSeederDisabled)),
            "Demo data seeder is disabled.");

    private static readonly Action<ILogger, Guid, string, Exception?> LogDemoSeedCompleted =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Information,
            new EventId(2, nameof(LogDemoSeedCompleted)),
            "Demo data seed completed for tenant {TenantId} with slug {TenantSlug}.");
}
