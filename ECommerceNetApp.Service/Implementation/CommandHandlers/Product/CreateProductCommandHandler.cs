using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Product
{
    public class CreateProductCommandHandler(IProductCatalogUnitOfWork productCatalogUnitOfWork)
        : ICommandHandler<CreateProductCommand, int>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;

        public async Task<int> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);

            var category = await _productCatalogUnitOfWork.CategoryRepository.GetByIdAsync(command.CategoryId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                throw new InvalidOperationException($"Category with id {command.CategoryId} not found");
            }

            var product = ProductEntity.Create(
                command.Name,
                command.Description,
                command.ImageUrl != null ? ImageInfo.Create(command.ImageUrl) : null,
                category!,
                Money.Create(command.Price, command.Currency),
                command.Amount);

            await _productCatalogUnitOfWork.ProductRepository.AddAsync(product, cancellationToken).ConfigureAwait(false);
            await _productCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
            return product.Id;
        }
    }
}
