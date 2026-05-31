using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Scheduling.Application.WorkingHours;
using ReserveFlow.Modules.Scheduling.Domain.UnavailablePeriods;

namespace ReserveFlow.Modules.Scheduling.Application.UnavailablePeriods.CreateUnavailablePeriod;

public sealed class CreateUnavailablePeriodCommandHandler(
    IUnavailablePeriodRepository unavailablePeriodRepository,
    ISchedulingUnitOfWork unitOfWork) : ICommandHandler<CreateUnavailablePeriodCommand, UnavailablePeriodResponse>
{
    public async Task<UnavailablePeriodResponse> Handle(
        CreateUnavailablePeriodCommand command,
        CancellationToken cancellationToken = default)
    {
        UnavailablePeriod unavailablePeriod = UnavailablePeriod.Create(
            command.TenantId,
            command.StaffMemberId,
            command.ResourceId,
            command.StartsAtUtc,
            command.EndsAtUtc,
            command.Reason);

        unavailablePeriodRepository.Insert(unavailablePeriod);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnavailablePeriodResponse.FromUnavailablePeriod(unavailablePeriod);
    }
}
