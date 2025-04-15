using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Queries.Product;

namespace ECommerceNetApp.Service.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(GetAllProductsQuery query)
        {
            var products = await _productRepository.GetAllAsync().ConfigureAwait(false);
            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryIdAsync(GetProductsByCategoryQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);
            var products = await _productRepository.GetProductsByCategoryIdAsync(query.CategoryId).ConfigureAwait(false);
            return products.Select(MapToDto);
        }

        public async Task<ProductDto?> GetProductByIdAsync(GetProductByIdQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);
            var product = await _productRepository.GetByIdAsync(query.Id).ConfigureAwait(false);
            return product != null ? MapToDto(product) : null;
        }

        public async Task<ProductDto> AddProductAsync(CreateProductCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            await ValidateProductAsync(command).ConfigureAwait(false);

            var category = await _categoryRepository.GetByIdAsync(command.CategoryId).ConfigureAwait(false);
            if (category == null)
            {
                throw new ArgumentException($"Category with ID {command.CategoryId} not found.");
            }

            var product = MapToDomain(command, category);
            var result = await _productRepository.AddAsync(product).ConfigureAwait(false);

            return MapToDto(result);
        }

        public async Task UpdateProductAsync(UpdateProductCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            await ValidateProductAsync(command).ConfigureAwait(false);

            var category = await _categoryRepository.GetByIdAsync(command.CategoryId).ConfigureAwait(false);
            if (category == null)
            {
                throw new ArgumentException($"Category with ID {command.CategoryId} not found.");
            }

            var existingProduct = await _productRepository.GetByIdAsync(command.Id).ConfigureAwait(false);

            if (existingProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID {command.Id} not found.");
            }

            var product = MapToDomain(command, category);
            await _productRepository.UpdateAsync(product).ConfigureAwait(false);
        }

        public async Task DeleteProductAsync(DeleteProductCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            var existingProduct = await _productRepository.GetByIdAsync(command.Id).ConfigureAwait(false);

            if (existingProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID {command.Id} not found.");
            }

            await _productRepository.DeleteAsync(command.Id).ConfigureAwait(false);
        }

        private static ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                Price = product.Price,
                Amount = product.Amount,
            };
        }

        private static Product MapToDomain(CreateProductCommand dto, Category category)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentNullException.ThrowIfNull(category);
            return new Product(
                dto.Name,
                dto.Description,
                dto.ImageUrl,
                category,
                dto.Price,
                dto.Amount);
        }

        private static Product MapToDomain(UpdateProductCommand dto, Category category)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentNullException.ThrowIfNull(category);
            return new Product(
                dto.Id,
                dto.Name,
                dto.Description,
                dto.ImageUrl,
                category,
                dto.Price,
                dto.Amount);
        }

        private async Task ValidateProductAsync(CreateProductCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            if (string.IsNullOrEmpty(command.Name))
            {
                throw new ArgumentException("Product name is required.");
            }

            if (command.Name.Length > 50)
            {
                throw new ArgumentException("Product name cannot exceed 50 characters.");
            }

            if (command.Price <= 0)
            {
                throw new ArgumentException("Product price must be greater than zero.");
            }

            if (command.Amount <= 0)
            {
                throw new ArgumentException("Product amount must be greater than zero.");
            }

            // Validate that category exists
            var category = await _categoryRepository.GetByIdAsync(command.CategoryId).ConfigureAwait(false);
            if (category == null)
            {
                throw new ArgumentException($"Category with ID {command.CategoryId} not found.");
            }
        }

        private async Task ValidateProductAsync(UpdateProductCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            if (string.IsNullOrEmpty(command.Name))
            {
                throw new ArgumentException("Product name is required.");
            }

            if (command.Name.Length > 50)
            {
                throw new ArgumentException("Product name cannot exceed 50 characters.");
            }

            if (command.Price <= 0)
            {
                throw new ArgumentException("Product price must be greater than zero.");
            }

            if (command.Amount <= 0)
            {
                throw new ArgumentException("Product amount must be greater than zero.");
            }

            // Validate that category exists
            var category = await _categoryRepository.GetByIdAsync(command.CategoryId).ConfigureAwait(false);
            if (category == null)
            {
                throw new ArgumentException($"Category with ID {command.CategoryId} not found.");
            }
        }
    }
}
