using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Category
{
    public class CreateCategoryCommandHandler(ProductCatalogDbContext dbContext)
        : ICommandHandler<CreateCategoryCommand, int>
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task<int> HandleAsync(CreateCategoryCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);
            CategoryEntity? parentCategory = null;

            if (command.ParentCategoryId.HasValue)
            {
                parentCategory = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == command.ParentCategoryId.Value, cancellationToken).ConfigureAwait(false);
                if (parentCategory == null)
                {
                    throw new InvalidOperationException($"Parent category with id {command.ParentCategoryId.Value} not found");
                }
            }

            var imageInfo = command.ImageUrl != null ? ImageInfo.Create(command.ImageUrl, null) : null;
            var category = CategoryEntity.Create(command.Name, imageInfo, parentCategory);

            await _dbContext.Categories.AddAsync(category, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return category.Id;
        }
    }
}
