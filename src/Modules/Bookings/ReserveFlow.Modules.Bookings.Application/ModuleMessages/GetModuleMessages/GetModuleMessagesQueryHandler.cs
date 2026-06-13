using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;

namespace ReserveFlow.Modules.Bookings.Application.ModuleMessages.GetModuleMessages;

public sealed class GetModuleMessagesQueryHandler(IModuleMessageRepository moduleMessageRepository)
    : IQueryHandler<GetModuleMessagesQuery, PagedResponse<ModuleMessageResponse>>
{
    public async Task<PagedResponse<ModuleMessageResponse>> Handle(
        GetModuleMessagesQuery query,
        CancellationToken cancellationToken = default)
    {
        PageRequest pageRequest = PageRequest.Create(query.PageNumber, query.PageSize);
        PagedResult<ModuleMessageResponse> messages = await moduleMessageRepository.GetByTenantIdAsync(
            query.TenantId,
            pageRequest,
            query.Status,
            query.Type,
            query.SortBy,
            query.SortDirection,
            cancellationToken);

        return new PagedResponse<ModuleMessageResponse>(
            messages.Items,
            pageRequest.PageNumber,
            pageRequest.PageSize,
            messages.TotalCount);
    }
}
