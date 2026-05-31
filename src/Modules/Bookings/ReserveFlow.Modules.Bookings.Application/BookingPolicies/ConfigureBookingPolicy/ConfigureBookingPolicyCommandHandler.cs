using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Bookings.Domain.BookingPolicies;
using ReserveFlow.Modules.Bookings.Application.Bookings;

namespace ReserveFlow.Modules.Bookings.Application.BookingPolicies.ConfigureBookingPolicy;

public sealed class ConfigureBookingPolicyCommandHandler(
    IBookingPolicyRepository bookingPolicyRepository,
    IBookingsUnitOfWork unitOfWork) : ICommandHandler<ConfigureBookingPolicyCommand, BookingPolicyResponse>
{
    public async Task<BookingPolicyResponse> Handle(
        ConfigureBookingPolicyCommand command,
        CancellationToken cancellationToken = default)
    {
        BookingPolicy? policy = await bookingPolicyRepository.GetByTenantIdAsync(
            command.TenantId,
            cancellationToken);

        if (policy is null)
        {
            policy = BookingPolicy.Configure(
                command.TenantId,
                command.MinimumAdvanceMinutes,
                command.CancellationDeadlineHours);

            bookingPolicyRepository.Insert(policy);
        }
        else
        {
            policy.Update(
                command.MinimumAdvanceMinutes,
                command.CancellationDeadlineHours);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return BookingPolicyResponse.FromPolicy(policy);
    }
}
