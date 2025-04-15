using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Product;
using MediatR;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using CategoryEntity = ECommerceNetApp.Domain.Entities.Category;
using ProductEntity = ECommerceNetApp.Domain.Entities.Product;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Product
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public CreateProductCommandHandler(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken).ConfigureAwait(false);

            ValidateCommandParameters(request, category);

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

        private static void ValidateCommandParameters(CreateProductCommand command, CategoryEntity? category)
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

            if (category == null)
            {
                throw new InvalidOperationException($"Category with id {command.CategoryId} not found");
            }
        }
    }
}
