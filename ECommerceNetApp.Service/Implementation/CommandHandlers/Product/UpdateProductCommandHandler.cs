using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Product;
using FluentValidation;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Product
{
    public class UpdateProductCommandHandler(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IValidator<UpdateProductCommand> validator)
        : IRequestHandler<UpdateProductCommand>
    {
        private readonly IProductRepository _productRepository = productRepository;
        private readonly ICategoryRepository _categoryRepository = categoryRepository;
        private readonly IValidator<UpdateProductCommand> _validator = validator;

        public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
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
            product.UpdateImage(request.ImageUrl);
            product.UpdatePrice(request.Price);
            product.UpdateAmount(request.Amount);
            await UpdateProductCategoryAsync(request, product, cancellationToken).ConfigureAwait(false);

            await _productRepository.UpdateAsync(product, cancellationToken).ConfigureAwait(false);
        }

        private async Task UpdateProductCategoryAsync(UpdateProductCommand command, ProductEntity product, CancellationToken cancellationToken)
        {
            if (command.CategoryId != product.CategoryId)
            {
                var category = await _categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken).ConfigureAwait(false);
                if (category == null)
                {
                    throw new InvalidOperationException($"Category with id {command.CategoryId} not found");
                }

                product.UpdateCategory(category);
            }
        }
    }
}
