using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Implementation.QueryHandlers.Product;
using ECommerceNetApp.Service.Queries.Product;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.QueryHandlers.Category
{
    public class GetAllProductsQueryHandlerTest
    {
        private readonly GetAllProductsQueryHandler _queryHandler;
        private readonly Mock<ProductCatalogDbContext> _mockDbContext;

        public GetAllProductsQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockDbContext = new Mock<ProductCatalogDbContext>(new DbContextOptions<ProductCatalogDbContext>());
            _queryHandler = new GetAllProductsQueryHandler(_mockDbContext.Object);
        }

        [Fact]
        public async Task GetAllCategories_ReturnsCategories()
        {
            var category = CategoryEntity.Create("Electronics", null, null, 1);

            // Arrange
            var products = new List<ProductEntity>
            {
                ProductEntity.Create("Laptop", null, null, category, Money.From(999.99m), 10, 1),
                ProductEntity.Create("Smartphone", null, null, category, Money.From(499.99m), 20, 2),
            }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Products).ReturnsDbSet(products);

            // Act
            var result = await _queryHandler.HandleAsync(new GetAllProductsQuery(), CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Count().ShouldBe(2);
            result.ShouldContain(c => c.Id == 1 && c.Name == "Laptop");
            result.ShouldContain(c => c.Id == 2 && c.Name == "Smartphone");
        }
    }
}
