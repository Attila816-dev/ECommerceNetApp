using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Category;
using MediatR;
using CategoryEntity = ECommerceNetApp.Domain.Entities.Category;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Category
{
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
    {
        private readonly ICategoryRepository _categoryRepository;

        public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var existingCategory = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (existingCategory == null)
            {
                throw new InvalidOperationException($"Category with Id {request.Id} not found");
            }

            ValidateCommandParameters(request);

            existingCategory.UpdateName(request.Name);
            existingCategory.UpdateImage(request.ImageUrl);
            await UpdateParentCategoryAsync(request, existingCategory, cancellationToken).ConfigureAwait(false);
            await _categoryRepository.UpdateAsync(existingCategory, cancellationToken).ConfigureAwait(false);
        }

        private static void ValidateCommandParameters(UpdateCategoryCommand request)
        {
            ArgumentException.ThrowIfNullOrEmpty(request.Name, "Category Name");

            if (request.Name.Length > CategoryEntity.MaxCategoryNameLength)
            {
                throw new ArgumentException($"Category name cannot exceed {CategoryEntity.MaxCategoryNameLength} characters.");
            }
        }

        private async Task UpdateParentCategoryAsync(UpdateCategoryCommand request, CategoryEntity category, CancellationToken cancellationToken)
        {
            if (request.ParentCategoryId != category.ParentCategoryId)
            {
                CategoryEntity? parentCategory = null;
                if (request.ParentCategoryId.HasValue)
                {
                    parentCategory = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken).ConfigureAwait(false);
                    if (parentCategory == null)
                    {
                        throw new InvalidOperationException($"Parent category with id {request.ParentCategoryId.Value} not found");
                    }
                }

                category.UpdateParentCategory(parentCategory);
            }
        }
    }
}
