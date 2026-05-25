using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Audit.Domain.AuditLogs;

namespace ReserveFlow.Modules.Audit.Application.AuditLogs.RecordAuditLog;

public sealed class RecordAuditLogCommandHandler(
    IAuditLogRepository auditLogRepository,
    IAuditUnitOfWork unitOfWork) : ICommandHandler<RecordAuditLogCommand, AuditLogResponse>
{
    public async Task<AuditLogResponse> Handle(
        RecordAuditLogCommand command,
        CancellationToken cancellationToken = default)
    {
        AuditLog log = AuditLog.Record(
            command.TenantId,
            command.UserId,
            command.Action,
            command.EntityName,
            command.EntityId,
            command.DetailsJson);

        auditLogRepository.Insert(log);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AuditLogResponse.FromAuditLog(log);
    }
}
