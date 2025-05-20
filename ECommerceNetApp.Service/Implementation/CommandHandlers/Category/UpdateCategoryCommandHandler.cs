using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Exceptions.Category;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Category
{
    public class UpdateCategoryCommandHandler(ProductCatalogDbContext dbContext)
        : ICommandHandler<UpdateCategoryCommand>
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext;

        public async Task HandleAsync(UpdateCategoryCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);

            var existingCategory = (await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken).ConfigureAwait(false))
                ?? throw InvalidCategoryException.NotFound(command.Id);

            var imageInfo = command.ImageUrl != null ? ImageInfo.Create(command.ImageUrl) : null;
            var parentCategory = await GetParentCategoryAsync(command, existingCategory, cancellationToken).ConfigureAwait(false);
            existingCategory.Update(command.Name, imageInfo, parentCategory);

            _dbContext.Categories.Update(existingCategory);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<CategoryEntity?> GetParentCategoryAsync(UpdateCategoryCommand command, CategoryEntity category, CancellationToken cancellationToken)
        {
            CategoryEntity? parentCategory = null;
            if (command.ParentCategoryId.HasValue)
            {
                parentCategory = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == command.ParentCategoryId.Value, cancellationToken).ConfigureAwait(false);
                if (parentCategory == null)
                {
                    throw new InvalidOperationException($"Parent category with id {command.ParentCategoryId.Value} not found");
                }
            }

            return parentCategory;
        }
    }
}
