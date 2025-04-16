using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Product;
using FluentValidation;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Product
{
    public class CreateProductCommandHandler(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IValidator<CreateProductCommand> validator) : IRequestHandler<CreateProductCommand, int>
    {
        private readonly IProductRepository _productRepository = productRepository;
        private readonly ICategoryRepository _categoryRepository = categoryRepository;
        private readonly IValidator<CreateProductCommand> _validator = validator;

        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                throw new InvalidOperationException($"Category with id {request.CategoryId} not found");
            }

            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var product = new ProductEntity(
                request.Name,
                request.Description,
                request.ImageUrl,
                category!,
                request.Price,
                request.Amount);

            await _productRepository.AddAsync(product, cancellationToken).ConfigureAwait(false);
            return product.Id;
        }
    }
}
