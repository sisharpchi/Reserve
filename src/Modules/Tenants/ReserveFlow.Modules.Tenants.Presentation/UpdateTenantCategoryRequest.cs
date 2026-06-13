namespace ReserveFlow.Modules.Tenants.Presentation;

public sealed record UpdateTenantCategoryRequest(
    string Name,
    string Slug,
    int SortOrder);
