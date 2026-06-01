using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Tenants.Application.Tenants.GetPlatformUsage;

public sealed record GetPlatformUsageQuery : IQuery<PlatformUsageResponse>;
