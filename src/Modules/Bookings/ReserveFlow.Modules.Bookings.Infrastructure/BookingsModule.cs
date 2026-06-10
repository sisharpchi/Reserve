using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Infrastructure.Outbox;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.BookingPolicies;
using ReserveFlow.Modules.Bookings.Application.BookingPolicies.ConfigureBookingPolicy;
using ReserveFlow.Modules.Bookings.Application.BookingHistory;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.CancelBooking;
using ReserveFlow.Modules.Bookings.Application.Bookings.CancelPublicBooking;
using ReserveFlow.Modules.Bookings.Application.Bookings.CompleteBooking;
using ReserveFlow.Modules.Bookings.Application.Bookings.ConfirmBooking;
using ReserveFlow.Modules.Bookings.Application.Bookings.CreateBooking;
using ReserveFlow.Modules.Bookings.Application.Bookings.ExpirePendingBooking;
using ReserveFlow.Modules.Bookings.Application.Bookings.GetBooking;
using ReserveFlow.Modules.Bookings.Application.Bookings.GetBookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.GetPublicBooking;
using ReserveFlow.Modules.Bookings.Application.Bookings.GetStaffSchedule;
using ReserveFlow.Modules.Bookings.Application.Bookings.MarkBookingAsNoShow;
using ReserveFlow.Modules.Bookings.Application.Bookings.PublicRescheduleBooking;
using ReserveFlow.Modules.Bookings.Application.Bookings.RescheduleBooking;
using ReserveFlow.Modules.Bookings.Application.ModuleMessages;
using ReserveFlow.Modules.Bookings.Application.ModuleMessages.GetModuleMessages;
using ReserveFlow.Modules.Bookings.Infrastructure.Bookings.Availability;
using ReserveFlow.Modules.Bookings.Infrastructure.Bookings.TenantStatus;
using ReserveFlow.Modules.Bookings.Application.Customers;
using ReserveFlow.Modules.Bookings.Infrastructure.BookingPolicies;
using ReserveFlow.Modules.Bookings.Infrastructure.BookingHistory;
using ReserveFlow.Modules.Bookings.Infrastructure.Bookings;
using ReserveFlow.Modules.Bookings.Infrastructure.Customers;
using ReserveFlow.Modules.Bookings.Infrastructure.Database;
using ReserveFlow.Modules.Bookings.Infrastructure.ModuleMessages;
using ReserveFlow.Modules.Bookings.Infrastructure.Outbox;

namespace ReserveFlow.Modules.Bookings.Infrastructure;

public static class BookingsModule
{
    public static IServiceCollection AddBookingsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<BookingsDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Database"),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Bookings)));

        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IBookingAvailabilityChecker, SchedulingBookingAvailabilityChecker>();
        services.AddScoped<ITenantBookingGate, PostgresTenantBookingGate>();
        services.AddScoped<IBookingPolicyRepository, BookingPolicyRepository>();
        services.AddScoped<IBookingHistoryRepository, BookingHistoryRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IModuleMessageRepository, ModuleMessageRepository>();
        services.AddScoped<IBookingsUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<BookingsDbContext>());
        services.AddScoped<ICommandHandler<ConfigureBookingPolicyCommand, BookingPolicyResponse>, ConfigureBookingPolicyCommandHandler>();
        services.AddScoped<ICommandHandler<CreateBookingCommand, BookingResponse>, CreateBookingCommandHandler>();
        services.AddScoped<IQueryHandler<GetBookingQuery, BookingResponse?>, GetBookingQueryHandler>();
        services.AddScoped<IQueryHandler<GetBookingsQuery, IReadOnlyList<BookingResponse>>, GetBookingsQueryHandler>();
        services.AddScoped<IQueryHandler<GetPublicBookingQuery, BookingResponse?>, GetPublicBookingQueryHandler>();
        services.AddScoped<IQueryHandler<GetStaffScheduleQuery, IReadOnlyList<BookingResponse>>, GetStaffScheduleQueryHandler>();
        services.AddScoped<IQueryHandler<GetModuleMessagesQuery, IReadOnlyList<ModuleMessageResponse>>, GetModuleMessagesQueryHandler>();
        services.AddScoped<ICommandHandler<CancelPublicBookingCommand, BookingResponse?>, CancelPublicBookingCommandHandler>();
        services.AddScoped<ICommandHandler<PublicRescheduleBookingCommand, BookingResponse?>, PublicRescheduleBookingCommandHandler>();
        services.AddScoped<ICommandHandler<CancelBookingCommand, BookingResponse?>, CancelBookingCommandHandler>();
        services.AddScoped<ICommandHandler<RescheduleBookingCommand, BookingResponse?>, RescheduleBookingCommandHandler>();
        services.AddScoped<ICommandHandler<ConfirmBookingCommand, BookingResponse?>, ConfirmBookingCommandHandler>();
        services.AddScoped<ICommandHandler<ExpirePendingBookingCommand, BookingResponse?>, ExpirePendingBookingCommandHandler>();
        services.AddScoped<ICommandHandler<CompleteBookingCommand, BookingResponse?>, CompleteBookingCommandHandler>();
        services.AddScoped<ICommandHandler<MarkBookingAsNoShowCommand, BookingResponse?>, MarkBookingAsNoShowCommandHandler>();
        services.Configure<BookingsOutboxOptions>(configuration.GetSection("Bookings:Outbox"));
        services.AddScoped<IOutboxMessageDispatcher, BookingNotificationOutboxMessageDispatcher>();
        services.AddHostedService<BookingsOutboxProcessorHostedService>();
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }
}
