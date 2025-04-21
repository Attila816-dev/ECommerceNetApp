using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.Interfaces.Mappers.Category;
using FluentValidation;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Category
{
    public class CreateCategoryCommandHandler(
            IProductCatalogUnitOfWork productCatalogUnitOfWork,
            ICategoryMapper categoryMapper,
            IValidator<CreateCategoryCommand> validator)
        : IRequestHandler<CreateCategoryCommand, int>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;
        private readonly ICategoryMapper _categoryMapper = categoryMapper;
        private readonly IValidator<CreateCategoryCommand> _validator = validator;

        public async Task<int> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            CategoryEntity? parentCategory = null;
            if (request.ParentCategoryId.HasValue)
            {
                parentCategory = await _productCatalogUnitOfWork.CategoryRepository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken: cancellationToken).ConfigureAwait(false);
                if (parentCategory == null)
                {
                    throw new InvalidOperationException($"Parent category with id {request.ParentCategoryId.Value} not found");
                }
            }

            var category = _categoryMapper.MapToEntity(request, parentCategory);
            await _productCatalogUnitOfWork.CategoryRepository.AddAsync(category, cancellationToken).ConfigureAwait(false);
            await _productCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

            return category.Id;
        }
    }
}
