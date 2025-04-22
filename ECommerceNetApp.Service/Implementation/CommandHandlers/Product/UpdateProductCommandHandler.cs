using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;
using FluentValidation;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Product
{
    public class UpdateProductCommandHandler(
        IProductCatalogUnitOfWork productCatalogUnitOfWork,
        IValidator<UpdateProductCommand> validator)
        : IRequestHandler<UpdateProductCommand>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;
        private readonly IValidator<UpdateProductCommand> _validator = validator;

        public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var product = await _productCatalogUnitOfWork.ProductRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (product == null)
            {
                throw new InvalidOperationException($"Product with id {request.Id} not found");
            }

            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            product.UpdateName(request.Name);
            product.UpdateDescription(request.Description);
            product.UpdateImage(request.ImageUrl != null ? new ImageInfo(request.ImageUrl, null) : null);
            product.UpdatePrice(new Money(request.Price, request.Currency));
            product.UpdateAmount(request.Amount);
            await UpdateProductCategoryAsync(request, product, cancellationToken).ConfigureAwait(false);

            _productCatalogUnitOfWork.ProductRepository.Update(product);
            await _productCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task UpdateProductCategoryAsync(UpdateProductCommand command, ProductEntity product, CancellationToken cancellationToken)
        {
            if (command.CategoryId != product.CategoryId)
            {
                var category = await _productCatalogUnitOfWork.CategoryRepository.GetByIdAsync(command.CategoryId, cancellationToken).ConfigureAwait(false);
                if (category == null)
                {
                    throw new InvalidOperationException($"Category with id {command.CategoryId} not found");
                }

                product.UpdateCategory(category);
            }
        }
    }
}
