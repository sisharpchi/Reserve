namespace ReserveFlow.Modules.Bookings.Application.ModuleMessages;

public sealed record ModuleMessageResponse(
    Guid Id,
    Guid TenantId,
    string Module,
    string Type,
    DateTime OccurredOnUtc,
    DateTime? ProcessedOnUtc,
    string Status,
    string? Error,
    int RetryCount);
