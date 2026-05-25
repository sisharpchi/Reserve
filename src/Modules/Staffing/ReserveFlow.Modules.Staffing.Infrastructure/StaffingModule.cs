using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Staffing.Application.StaffMembers;
using ReserveFlow.Modules.Staffing.Application.StaffMembers.CreateStaffMember;
using ReserveFlow.Modules.Staffing.Application.StaffMembers.GetActiveStaffMembers;
using ReserveFlow.Modules.Staffing.Infrastructure.Database;
using ReserveFlow.Modules.Staffing.Infrastructure.StaffMembers;

namespace ReserveFlow.Modules.Staffing.Infrastructure;

public static class StaffingModule
{
    public static IServiceCollection AddStaffingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<StaffingDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Database"),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Staffing)));

        services.AddScoped<IStaffMemberRepository, StaffMemberRepository>();
        services.AddScoped<IStaffingUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<StaffingDbContext>());
        services.AddScoped<ICommandHandler<CreateStaffMemberCommand, StaffMemberResponse>, CreateStaffMemberCommandHandler>();
        services.AddScoped<IQueryHandler<GetActiveStaffMembersQuery, IReadOnlyList<StaffMemberResponse>>, GetActiveStaffMembersQueryHandler>();
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }
}
