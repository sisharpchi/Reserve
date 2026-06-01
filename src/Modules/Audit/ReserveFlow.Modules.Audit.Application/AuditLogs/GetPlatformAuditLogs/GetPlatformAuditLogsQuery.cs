using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Audit.Application.AuditLogs.GetPlatformAuditLogs;

public sealed record GetPlatformAuditLogsQuery(int Limit = 100)
    : IQuery<IReadOnlyList<AuditLogResponse>>;
