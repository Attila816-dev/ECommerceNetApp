using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.Interfaces.Mappers.Product;
using FluentValidation;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Product
{
    public class CreateProductCommandHandler(
            IProductCatalogUnitOfWork productCatalogUnitOfWork,
            IProductMapper productMapper,
            IValidator<CreateProductCommand> validator) : IRequestHandler<CreateProductCommand, int>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;
        private readonly IProductMapper _productMapper = productMapper;
        private readonly IValidator<CreateProductCommand> _validator = validator;

        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var category = await _productCatalogUnitOfWork.CategoryRepository.GetByIdAsync(request.CategoryId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                throw new InvalidOperationException($"Category with id {request.CategoryId} not found");
            }

            var productEntity = _productMapper.MapToEntity(request, category);
            await _productCatalogUnitOfWork.ProductRepository.AddAsync(productEntity, cancellationToken).ConfigureAwait(false);
            await _productCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
            return productEntity.Id;
        }
    }
}
