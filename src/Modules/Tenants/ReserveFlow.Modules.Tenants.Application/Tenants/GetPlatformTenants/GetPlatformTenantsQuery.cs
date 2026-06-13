using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;

namespace ReserveFlow.Modules.Tenants.Application.Tenants.GetPlatformTenants;

public sealed record GetPlatformTenantsQuery(
    int? PageNumber,
    int? PageSize,
    Guid? CategoryId,
    string? Search,
    string? Status,
    string? SortBy,
    string? SortDirection) : IQuery<PagedResponse<TenantResponse>>;
