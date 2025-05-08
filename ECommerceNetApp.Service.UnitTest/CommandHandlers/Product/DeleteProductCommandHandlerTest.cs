using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Exceptions.Product;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Product;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Product
{
    public class DeleteProductCommandHandlerTest
    {
        private readonly DeleteProductCommandHandler _commandHandler;
        private readonly Mock<ProductCatalogDbContext> _mockDbContext;

        public DeleteProductCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            var mockDomainEventService = new Mock<IDomainEventService>();
            _mockDbContext = new Mock<ProductCatalogDbContext>(new DbContextOptions<ProductCatalogDbContext>(), mockDomainEventService.Object);
            _commandHandler = new DeleteProductCommandHandler(_mockDbContext.Object);
        }

        [Fact]
        public async Task DeleteExistingProduct_RemovesProduct()
        {
            // Arrange
            var category = CategoryEntity.Create("test-category", null, null, 1);
            var product = ProductEntity.Create("test-product", null, null, category, Money.From(10), 2, 1);

            var products = new List<ProductEntity> { product }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Products).ReturnsDbSet(products);

            var categories = new List<CategoryEntity> { category }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            // Act
            await _commandHandler.HandleAsync(new DeleteProductCommand(product.Id), CancellationToken.None);

            // Assert
            _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteProductAsync_WithNonExistingProduct_ShouldThrowInvalidProductException()
        {
            // Arrange
            var productId = 2;

            var products = new List<ProductEntity>().AsQueryable();
            _mockDbContext.SetupGet(c => c.Products).ReturnsDbSet(products);

            var categories = new List<CategoryEntity>().AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            // Act & Assert
            await Should.ThrowAsync<InvalidProductException>(() =>
                _commandHandler.HandleAsync(new DeleteProductCommand(productId), CancellationToken.None));
        }
    }
}
