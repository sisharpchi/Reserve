using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Scheduling.Application.UnavailablePeriods.CreateUnavailablePeriod;

public sealed record CreateUnavailablePeriodCommand(
    Guid TenantId,
    Guid? StaffMemberId,
    Guid? ResourceId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string? Reason) : ICommand<UnavailablePeriodResponse>;
