using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Category;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Category
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;

        public DeleteCategoryCommandHandler(
            ICategoryRepository categoryRepository,
            IProductRepository productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            await ValidateCategoryBeforeDeleteAsync(request, cancellationToken).ConfigureAwait(false);
            await _categoryRepository.DeleteAsync(request.Id, cancellationToken).ConfigureAwait(false);
        }

        private async Task ValidateCategoryBeforeDeleteAsync(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var exists = await _categoryRepository.ExistsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (!exists)
            {
                throw new InvalidOperationException($"Category with id {request.Id} not found");
            }

            // Check if category has products
            var products = await _productRepository.GetProductsByCategoryIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (products != null && products.Any())
            {
                throw new InvalidOperationException("Cannot delete category with associated products");
            }

            // Check if category has subcategories
            var subcategories = await _categoryRepository.GetByParentCategoryIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (subcategories != null && subcategories.Any())
            {
                throw new InvalidOperationException("Cannot delete category with subcategories");
            }
        }
    }
}
