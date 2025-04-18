using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.Interfaces.Mappers.Product;
using FluentValidation;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Product
{
    public class CreateProductCommandHandler(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IProductMapper productMapper,
            IValidator<CreateProductCommand> validator) : IRequestHandler<CreateProductCommand, int>
    {
        private readonly IProductRepository _productRepository = productRepository;
        private readonly ICategoryRepository _categoryRepository = categoryRepository;
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

            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                throw new InvalidOperationException($"Category with id {request.CategoryId} not found");
            }

            var productEntity = _productMapper.MapToEntity(request, category);
            await _productRepository.AddAsync(productEntity, cancellationToken).ConfigureAwait(false);
            return productEntity.Id;
        }
    }
}
