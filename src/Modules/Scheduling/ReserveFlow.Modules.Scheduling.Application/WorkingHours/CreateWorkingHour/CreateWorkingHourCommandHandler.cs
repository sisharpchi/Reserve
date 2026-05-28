using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Scheduling.Domain.WorkingHours;

namespace ReserveFlow.Modules.Scheduling.Application.WorkingHours.CreateWorkingHour;

public sealed class CreateWorkingHourCommandHandler(
    IWorkingHourRepository workingHourRepository,
    ISchedulingUnitOfWork unitOfWork) : ICommandHandler<CreateWorkingHourCommand, WorkingHourResponse>
{
    public async Task<WorkingHourResponse> Handle(
        CreateWorkingHourCommand command,
        CancellationToken cancellationToken = default)
    {
        WorkingHour workingHour = WorkingHour.Create(
            command.TenantId,
            command.StaffMemberId,
            command.ResourceId,
            command.DayOfWeek,
            command.StartsAt,
            command.EndsAt);

        workingHourRepository.Insert(workingHour);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return WorkingHourResponse.FromWorkingHour(workingHour);
    }
}
