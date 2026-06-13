using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Tenants.Application.Tenants;
using ReserveFlow.Modules.Tenants.Domain.TenantCategories;

namespace ReserveFlow.Modules.Tenants.Application.TenantCategories.UpdateTenantCategory;

public sealed class UpdateTenantCategoryCommandHandler(
    ITenantCategoryRepository categoryRepository,
    ITenantsUnitOfWork unitOfWork) : ICommandHandler<UpdateTenantCategoryCommand, TenantCategoryResponse?>
{
    public async Task<TenantCategoryResponse?> Handle(
        UpdateTenantCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        TenantCategory? category = await categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);

        if (category is null)
        {
            return null;
        }

        string normalizedSlug = TenantCategory.NormalizeSlug(command.Slug);

        if (category.Slug != normalizedSlug &&
            await categoryRepository.ExistsBySlugAsync(normalizedSlug, cancellationToken))
        {
            throw new InvalidOperationException("Tenant category slug already exists.");
        }

        category.Update(command.Name, normalizedSlug, command.SortOrder);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TenantCategoryResponse.FromCategory(category);
    }
}
