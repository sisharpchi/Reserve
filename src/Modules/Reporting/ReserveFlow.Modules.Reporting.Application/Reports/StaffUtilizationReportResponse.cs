namespace ReserveFlow.Modules.Reporting.Application.Reports;

public sealed record StaffUtilizationReportResponse(
    Guid TenantId,
    DateOnly From,
    DateOnly To,
    IReadOnlyList<StaffUtilizationReportItem> Staff);

public sealed record StaffUtilizationReportItem(
    Guid StaffMemberId,
    string StaffMemberName,
    int TotalBookings,
    int CompletedBookings,
    int NoShowBookings,
    int CancelledBookings,
    int BookedMinutes,
    int UtilizedMinutes,
    decimal UtilizationRate)
{
    public static StaffUtilizationReportItem Create(
        Guid staffMemberId,
        string staffMemberName,
        int totalBookings,
        int completedBookings,
        int noShowBookings,
        int cancelledBookings,
        int bookedMinutes,
        int utilizedMinutes)
    {
        decimal utilizationRate = bookedMinutes == 0
            ? 0m
            : Math.Round((decimal)utilizedMinutes / bookedMinutes, 4);

        return new StaffUtilizationReportItem(
            staffMemberId,
            staffMemberName,
            totalBookings,
            completedBookings,
            noShowBookings,
            cancelledBookings,
            bookedMinutes,
            utilizedMinutes,
            utilizationRate);
    }
}
