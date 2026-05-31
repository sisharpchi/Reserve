namespace ReserveFlow.Modules.Tenants.Presentation;

internal sealed record CreateTenantCategoryRequest(
    string Name,
    string Slug,
    int SortOrder = 0);
