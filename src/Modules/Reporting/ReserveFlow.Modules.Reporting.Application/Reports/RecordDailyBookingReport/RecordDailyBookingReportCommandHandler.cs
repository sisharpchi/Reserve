using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Reporting.Domain.Reports;

namespace ReserveFlow.Modules.Reporting.Application.Reports.RecordDailyBookingReport;

public sealed class RecordDailyBookingReportCommandHandler(
    IReportingRepository reportingRepository,
    IReportingUnitOfWork unitOfWork) : ICommandHandler<RecordDailyBookingReportCommand, DailyBookingReportResponse>
{
    public async Task<DailyBookingReportResponse> Handle(
        RecordDailyBookingReportCommand command,
        CancellationToken cancellationToken = default)
    {
        DailyBookingReport? existingReport = await reportingRepository.GetDailyBookingReportAsync(
            command.TenantId,
            command.Date,
            cancellationToken);

        if (existingReport is not null)
        {
            existingReport.ReplaceCounts(
                command.CreatedBookings,
                command.CancelledBookings,
                command.CompletedBookings,
                command.NoShowBookings);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return DailyBookingReportResponse.FromReport(existingReport);
        }

        DailyBookingReport report = DailyBookingReport.Record(
            command.TenantId,
            command.Date,
            command.CreatedBookings,
            command.CancelledBookings,
            command.CompletedBookings,
            command.NoShowBookings);

        reportingRepository.Insert(report);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return DailyBookingReportResponse.FromReport(report);
    }
}
