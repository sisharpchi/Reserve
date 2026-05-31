using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Scheduling.Application.Availability;
using ReserveFlow.Modules.Scheduling.Application.Availability.GetAvailableSlots;
using ReserveFlow.Modules.Scheduling.Application.Availability.GetTenantAvailableSlots;
using ReserveFlow.Modules.Scheduling.Application.UnavailablePeriods;
using ReserveFlow.Modules.Scheduling.Application.UnavailablePeriods.CreateUnavailablePeriod;
using ReserveFlow.Modules.Scheduling.Application.WorkingHours;
using ReserveFlow.Modules.Scheduling.Application.WorkingHours.CreateWorkingHour;
using ReserveFlow.Modules.Scheduling.Infrastructure.Database;
using ReserveFlow.Modules.Scheduling.Infrastructure.UnavailablePeriods;
using ReserveFlow.Modules.Scheduling.Infrastructure.WorkingHours;

namespace ReserveFlow.Modules.Scheduling.Infrastructure;

public static class SchedulingModule
{
    public static IServiceCollection AddSchedulingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<SchedulingDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Database"),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Scheduling)));

        services.AddScoped<IUnavailablePeriodRepository, UnavailablePeriodRepository>();
        services.AddScoped<IWorkingHourRepository, WorkingHourRepository>();
        services.AddScoped<ISchedulingUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<SchedulingDbContext>());
        services.AddScoped<IQueryHandler<GetAvailableSlotsQuery, IReadOnlyList<AvailableSlotResponse>>, GetAvailableSlotsQueryHandler>();
        services.AddScoped<IQueryHandler<GetTenantAvailableSlotsQuery, IReadOnlyList<AvailableSlotResponse>>, GetTenantAvailableSlotsQueryHandler>();
        services.AddScoped<ICommandHandler<CreateUnavailablePeriodCommand, UnavailablePeriodResponse>, CreateUnavailablePeriodCommandHandler>();
        services.AddScoped<ICommandHandler<CreateWorkingHourCommand, WorkingHourResponse>, CreateWorkingHourCommandHandler>();
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }
}
