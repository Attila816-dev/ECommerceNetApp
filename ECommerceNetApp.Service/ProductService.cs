using ECommerceNetApp.Domain;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Interfaces;

namespace ECommerceNetApp.Service
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

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync().ConfigureAwait(false);
            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryIdAsync(int categoryId)
        {
            var products = await _productRepository.GetProductsByCategoryIdAsync(categoryId).ConfigureAwait(false);
            return products.Select(MapToDto);
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id).ConfigureAwait(false);
            return product != null ? MapToDto(product) : null;
        }

        public async Task<ProductDto> AddProductAsync(ProductDto productDto)
        {
            await ValidateProductAsync(productDto).ConfigureAwait(false);

            var product = MapToDomain(productDto);
            var result = await _productRepository.AddAsync(product).ConfigureAwait(false);

            return MapToDto(result);
        }

        public async Task UpdateProductAsync(ProductDto productDto)
        {
            await ValidateProductAsync(productDto).ConfigureAwait(false);

            var existingProduct = await _productRepository.GetByIdAsync(productDto.Id).ConfigureAwait(false);

            if (existingProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID {productDto.Id} not found.");
            }

            var product = MapToDomain(productDto);
            await _productRepository.UpdateAsync(product).ConfigureAwait(false);
        }

        public async Task DeleteProductAsync(int id)
        {
            var existingProduct = await _productRepository.GetByIdAsync(id).ConfigureAwait(false);

            if (existingProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found.");
            }

            await _productRepository.DeleteAsync(id).ConfigureAwait(false);
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
                Price = product.Price,
                Amount = product.Amount,
            };
        }

        private static Product MapToDomain(ProductDto dto)
        {
            return new Product
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                CategoryId = dto.CategoryId,
                Price = dto.Price,
                Amount = dto.Amount,
            };
        }

        private async Task ValidateProductAsync(ProductDto product)
        {
            ArgumentNullException.ThrowIfNull(product);

            if (string.IsNullOrEmpty(product.Name))
            {
                throw new ArgumentException("Product name is required.", nameof(product));
            }

            if (product.Name.Length > 50)
            {
                throw new ArgumentException("Product name cannot exceed 50 characters.", nameof(product));
            }

            if (product.Price <= 0)
            {
                throw new ArgumentException("Product price must be greater than zero.", nameof(product));
            }

            if (product.Amount <= 0)
            {
                throw new ArgumentException("Product amount must be greater than zero.", nameof(product));
            }

            // Validate that category exists
            var category = await _categoryRepository.GetByIdAsync(product.CategoryId).ConfigureAwait(false);
            if (category == null)
            {
                throw new ArgumentException($"Category with ID {product.CategoryId} not found.", nameof(product));
            }
        }
    }
}
