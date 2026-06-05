using Microsoft.EntityFrameworkCore;
using ReserveFlow.Common.Infrastructure.Outbox;
using ReserveFlow.Modules.Bookings.Application.ModuleMessages;
using ReserveFlow.Modules.Bookings.Infrastructure.Database;

namespace ReserveFlow.Modules.Bookings.Infrastructure.ModuleMessages;

internal sealed class ModuleMessageRepository(BookingsDbContext dbContext) : IModuleMessageRepository
{
    public async Task<IReadOnlyList<ModuleMessageResponse>> GetByTenantIdAsync(
        Guid tenantId,
        int take,
        CancellationToken cancellationToken = default)
    {
        OutboxMessage[] messages = await dbContext.OutboxMessages
            .AsNoTracking()
            .Where(message => message.TenantId == tenantId)
            .OrderByDescending(message => message.OccurredOnUtc)
            .Take(take)
            .ToArrayAsync(cancellationToken);

        return messages
            .Select(message => new ModuleMessageResponse(
                message.Id,
                tenantId,
                "Bookings",
                message.Type,
                message.OccurredOnUtc,
                message.ProcessedOnUtc,
                ResolveStatus(message),
                message.Error,
                message.RetryCount))
            .ToArray();
    }

    private static string ResolveStatus(OutboxMessage message)
    {
        if (message.ProcessedOnUtc is not null)
        {
            return "Processed";
        }

        return string.IsNullOrWhiteSpace(message.Error) ? "Pending" : "Failed";
    }
}
