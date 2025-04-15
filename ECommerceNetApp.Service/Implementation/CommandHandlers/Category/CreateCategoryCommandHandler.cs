using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Category;
using MediatR;
using CategoryEntity = ECommerceNetApp.Domain.Entities.Category;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Category
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, int>
    {
        private readonly ICategoryRepository _categoryRepository;

        public CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<int> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ValidateCommandParameters(request);

            CategoryEntity? parentCategory = null;
            if (request.ParentCategoryId.HasValue)
            {
                parentCategory = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken).ConfigureAwait(false);
                if (parentCategory == null)
                {
                    throw new InvalidOperationException($"Parent category with id {request.ParentCategoryId.Value} not found");
                }
            }

            var category = new CategoryEntity(request.Name, request.ImageUrl, parentCategory);
            await _categoryRepository.AddAsync(category, cancellationToken).ConfigureAwait(false);

            return category.Id;
        }

        private static void ValidateCommandParameters(CreateCategoryCommand request)
        {
            if (string.IsNullOrEmpty(request.Name))
            {
                throw new ArgumentException("Category name is required.");
            }

            if (request.Name.Length > CategoryEntity.MaxCategoryNameLength)
            {
                throw new ArgumentException($"Category name cannot exceed {CategoryEntity.MaxCategoryNameLength} characters.");
            }
        }
    }
}
