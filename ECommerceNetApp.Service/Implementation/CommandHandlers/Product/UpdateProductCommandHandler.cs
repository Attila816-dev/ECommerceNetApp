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

            var category = await _productCatalogUnitOfWork.CategoryRepository.GetByIdAsync(request.CategoryId, cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                throw new ArgumentException($"Category with id {request.CategoryId} not found");
            }

            var imageInfo = request.ImageUrl != null ? new ImageInfo(request.ImageUrl, null) : null;
            var money = new Money(request.Price, request.Currency);

            product.Update(request.Name, request.Description, imageInfo, category, money, request.Amount);
            _productCatalogUnitOfWork.ProductRepository.Update(product);
            await _productCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
