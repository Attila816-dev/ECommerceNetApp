using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Category
{
    public class DeleteCategoryCommandHandler(
        IProductCatalogUnitOfWork productCatalogUnitOfWork)
        : IRequestHandler<DeleteCategoryCommand>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;

        public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            // Get all products related to this category
            var products = await _productCatalogUnitOfWork.ProductRepository
                .GetProductsByCategoryIdAsync(request.Id, cancellationToken)
                .ConfigureAwait(false);

            // Delete all related products first
            foreach (var product in products)
            {
                await _productCatalogUnitOfWork.ProductRepository
                    .DeleteAsync(product.Id, cancellationToken)
                    .ConfigureAwait(false);
            }

            await _productCatalogUnitOfWork.CategoryRepository.DeleteAsync(request.Id, cancellationToken).ConfigureAwait(false);
            await _productCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
