using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;

namespace ReserveFlow.Modules.Catalog.Application.Services.GetServices;

public sealed record GetServicesQuery(
    Guid TenantId,
    int? PageNumber,
    int? PageSize,
    string? Search,
    bool? IsActive,
    string? SortBy,
    string? SortDirection) : IQuery<PagedResponse<ServiceResponse>>;
