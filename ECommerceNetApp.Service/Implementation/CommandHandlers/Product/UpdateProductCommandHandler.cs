using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Product
{
    public class UpdateProductCommandHandler(
        IProductCatalogUnitOfWork productCatalogUnitOfWork)
        : IRequestHandler<UpdateProductCommand>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;

        public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var product = await _productCatalogUnitOfWork.ProductRepository.GetByIdAsync(
                request.Id,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (product == null)
            {
                throw new InvalidOperationException($"Product with id {request.Id} not found");
            }

            var category = await _productCatalogUnitOfWork.CategoryRepository
                .GetByIdAsync(request.CategoryId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                throw new ArgumentException($"Category with id {request.CategoryId} not found");
            }

            var imageInfo = request.ImageUrl != null ? ImageInfo.Create(request.ImageUrl) : null;
            var money = Money.Create(request.Price, request.Currency);

            product.Update(request.Name, request.Description, imageInfo, category, money, request.Amount);
            _productCatalogUnitOfWork.ProductRepository.Update(product);
            await _productCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
