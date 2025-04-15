using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Product;
using MediatR;
using ProductEntity = ECommerceNetApp.Domain.Entities.Product;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Product
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public UpdateProductCommandHandler(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (product == null)
            {
                throw new InvalidOperationException($"Product with id {request.Id} not found");
            }

            ValidateCommandParameters(request);

            product.UpdateName(request.Name);
            product.UpdateDescription(request.Description);
            product.UpdateImage(request.ImageUrl);
            product.UpdatePrice(request.Price);
            product.UpdateAmount(request.Amount);
            await UpdateProductCategory(request, product, cancellationToken).ConfigureAwait(false);

            await _productRepository.UpdateAsync(product, cancellationToken).ConfigureAwait(false);
        }

        private static void ValidateCommandParameters(UpdateProductCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            if (string.IsNullOrEmpty(command.Name))
            {
                throw new ArgumentException("Product name is required.");
            }

            if (command.Name.Length > ProductEntity.MaxProductNameLength)
            {
                throw new ArgumentException($"Product name cannot exceed {ProductEntity.MaxProductNameLength} characters.");
            }

            if (command.Price <= 0)
            {
                throw new ArgumentException("Product price must be greater than zero.");
            }

            if (command.Amount <= 0)
            {
                throw new ArgumentException("Product amount must be greater than zero.");
            }
        }

        private async Task UpdateProductCategory(UpdateProductCommand command, Domain.Entities.Product product, CancellationToken cancellationToken)
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
