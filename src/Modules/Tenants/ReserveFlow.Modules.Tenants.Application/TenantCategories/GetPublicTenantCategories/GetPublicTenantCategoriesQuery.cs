using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Tenants.Application.TenantCategories.GetPublicTenantCategories;

public sealed record GetPublicTenantCategoriesQuery : IQuery<IReadOnlyList<TenantCategoryResponse>>;
