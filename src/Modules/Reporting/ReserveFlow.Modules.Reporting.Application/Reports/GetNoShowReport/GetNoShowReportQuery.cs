using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Reporting.Application.Reports.GetNoShowReport;

public sealed record GetNoShowReportQuery(
    Guid TenantId,
    DateOnly Date) : IQuery<NoShowReportResponse?>;
