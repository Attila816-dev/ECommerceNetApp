using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Product
{
    public class UpdateProductCommandHandler(
        IProductCatalogUnitOfWork productCatalogUnitOfWork)
        : ICommandHandler<UpdateProductCommand>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;

        public async Task HandleAsync(UpdateProductCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);

            var product = await _productCatalogUnitOfWork.ProductRepository.GetByIdAsync(
                command.Id,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (product == null)
            {
                throw new InvalidOperationException($"Product with id {command.Id} not found");
            }

            var category = await _productCatalogUnitOfWork.CategoryRepository
                .GetByIdAsync(command.CategoryId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                throw new ArgumentException($"Category with id {command.CategoryId} not found");
            }

            var imageInfo = command.ImageUrl != null ? ImageInfo.Create(command.ImageUrl) : null;
            var money = Money.Create(command.Price, command.Currency);

            product.Update(command.Name, command.Description, imageInfo, category, money, command.Amount);
            _productCatalogUnitOfWork.ProductRepository.Update(product);
            await _productCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
