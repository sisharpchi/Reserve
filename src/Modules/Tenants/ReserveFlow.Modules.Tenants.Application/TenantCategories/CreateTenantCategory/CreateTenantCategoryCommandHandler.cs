using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Tenants.Application.Tenants;
using ReserveFlow.Modules.Tenants.Domain.TenantCategories;

namespace ReserveFlow.Modules.Tenants.Application.TenantCategories.CreateTenantCategory;

public sealed class CreateTenantCategoryCommandHandler(
    ITenantCategoryRepository categoryRepository,
    ITenantsUnitOfWork unitOfWork) : ICommandHandler<CreateTenantCategoryCommand, TenantCategoryResponse>
{
    public async Task<TenantCategoryResponse> Handle(
        CreateTenantCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        string normalizedSlug = TenantCategory.NormalizeSlug(command.Slug);

        if (await categoryRepository.ExistsBySlugAsync(normalizedSlug, cancellationToken))
        {
            throw new InvalidOperationException("Tenant category slug already exists.");
        }

        TenantCategory category = TenantCategory.Create(
            command.Name,
            normalizedSlug,
            command.SortOrder);

        categoryRepository.Insert(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TenantCategoryResponse.FromCategory(category);
    }
}
