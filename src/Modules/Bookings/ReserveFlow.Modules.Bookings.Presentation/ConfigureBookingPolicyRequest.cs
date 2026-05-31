namespace ReserveFlow.Modules.Bookings.Presentation;

public sealed record ConfigureBookingPolicyRequest(
    int MinimumAdvanceMinutes,
    int CancellationDeadlineHours);
