using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;

namespace ReserveFlow.Modules.Bookings.Application.ModuleMessages.GetModuleMessages;

public sealed record GetModuleMessagesQuery(
    Guid TenantId,
    int? PageNumber,
    int? PageSize,
    string? Status,
    string? Type,
    string? SortBy,
    string? SortDirection) : IQuery<PagedResponse<ModuleMessageResponse>>;
