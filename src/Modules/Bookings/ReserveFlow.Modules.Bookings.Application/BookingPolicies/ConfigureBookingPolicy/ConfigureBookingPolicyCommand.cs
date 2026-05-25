using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Bookings.Application.BookingPolicies.ConfigureBookingPolicy;

public sealed record ConfigureBookingPolicyCommand(
    Guid TenantId,
    int MinimumAdvanceMinutes,
    int CancellationDeadlineHours) : ICommand<BookingPolicyResponse>;
