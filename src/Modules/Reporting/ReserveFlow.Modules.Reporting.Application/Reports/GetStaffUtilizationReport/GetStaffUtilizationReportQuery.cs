using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Reporting.Application.Reports.GetStaffUtilizationReport;

public sealed record GetStaffUtilizationReportQuery(
    Guid TenantId,
    DateOnly From,
    DateOnly To) : IQuery<StaffUtilizationReportResponse>;
