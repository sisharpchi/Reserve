namespace ReserveFlow.Common.Application.Abstractions;

public interface ITenantContext
{
    Guid? TenantId { get; }

    string? TenantSlug { get; }

    string? TimeZoneId { get; }

    bool IsPlatformScope { get; }
}
