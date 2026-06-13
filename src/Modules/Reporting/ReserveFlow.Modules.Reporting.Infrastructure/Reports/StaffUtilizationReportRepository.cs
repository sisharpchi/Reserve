using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Reporting.Application.Reports;
using ReserveFlow.Modules.Reporting.Infrastructure.Database;

namespace ReserveFlow.Modules.Reporting.Infrastructure.Reports;

internal sealed class StaffUtilizationReportRepository(ReportingDbContext dbContext)
    : IStaffUtilizationReportRepository
{
    private const string StaffUtilizationSql = """
        select
            b.staff_member_id,
            coalesce(s.display_name, 'Unknown staff') as staff_member_name,
            count(*)::int as total_bookings,
            count(*) filter (where b.status = 'Completed')::int as completed_bookings,
            count(*) filter (where b.status = 'NoShow')::int as no_show_bookings,
            count(*) filter (where b.status = 'Cancelled')::int as cancelled_bookings,
            coalesce(sum((extract(epoch from (b.ends_at_utc - b.starts_at_utc)) / 60))::int, 0) as booked_minutes,
            coalesce(sum(case
                when b.status in ('Completed', 'NoShow')
                then (extract(epoch from (b.ends_at_utc - b.starts_at_utc)) / 60)
                else 0
            end)::int, 0) as utilized_minutes
        from bookings.bookings b
        left join staffing.staff_members s
            on s.id = b.staff_member_id and s.tenant_id = b.tenant_id
        where b.tenant_id = @tenant_id
          and b.staff_member_id is not null
          and b.starts_at_utc >= @from_utc
          and b.starts_at_utc < @to_utc
        group by b.staff_member_id, s.display_name
        order by utilized_minutes desc, staff_member_name;
        """;

    public async Task<StaffUtilizationReportResponse> GetAsync(
        Guid tenantId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var items = new List<StaffUtilizationReportItem>();
        DbConnection connection = dbContext.Database.GetDbConnection();
        bool shouldCloseConnection = connection.State is not ConnectionState.Open;

        if (shouldCloseConnection)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using DbCommand command = connection.CreateCommand();
            command.CommandText = StaffUtilizationSql;

            AddParameter(command, "tenant_id", tenantId);
            AddParameter(command, "from_utc", fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
            AddParameter(command, "to_utc", toDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));

            await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                items.Add(StaffUtilizationReportItem.Create(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetInt32(2),
                    reader.GetInt32(3),
                    reader.GetInt32(4),
                    reader.GetInt32(5),
                    reader.GetInt32(6),
                    reader.GetInt32(7)));
            }
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }

        return new StaffUtilizationReportResponse(tenantId, fromDate, toDate, items);
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        DbParameter parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
