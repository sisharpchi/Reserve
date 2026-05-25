using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Reporting.Application.Reports;
using ReserveFlow.Modules.Reporting.Application.Reports.GetDailyBookingReport;
using ReserveFlow.Modules.Reporting.Application.Reports.RecordDailyBookingReport;
using ReserveFlow.Modules.Reporting.Infrastructure.Database;
using ReserveFlow.Modules.Reporting.Infrastructure.Reports;

namespace ReserveFlow.Modules.Reporting.Infrastructure;

public static class ReportingModule
{
    public static IServiceCollection AddReportingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ReportingDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Database"),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Reporting)));

        services.AddScoped<IReportingRepository, ReportingRepository>();
        services.AddScoped<IReportingUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<ReportingDbContext>());
        services.AddScoped<ICommandHandler<RecordDailyBookingReportCommand, DailyBookingReportResponse>, RecordDailyBookingReportCommandHandler>();
        services.AddScoped<IQueryHandler<GetDailyBookingReportQuery, DailyBookingReportResponse?>, GetDailyBookingReportQueryHandler>();
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }
}
