using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Tenants.Application.TenantCategories.GetPlatformTenantCategories;

public sealed record GetPlatformTenantCategoriesQuery : IQuery<IReadOnlyList<TenantCategoryResponse>>;
