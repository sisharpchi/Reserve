using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;

namespace ReserveFlow.Modules.Resources.Application.Resources.GetResources;

public sealed record GetResourcesQuery(
    Guid TenantId,
    int? PageNumber,
    int? PageSize,
    string? Search,
    bool? IsActive,
    string? ResourceType,
    string? SortBy,
    string? SortDirection) : IQuery<PagedResponse<ResourceResponse>>;
