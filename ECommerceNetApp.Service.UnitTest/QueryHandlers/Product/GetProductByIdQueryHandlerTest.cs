using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Implementation.QueryHandlers.Product;
using ECommerceNetApp.Service.Queries.Product;
using ECommerceNetApp.Service.UnitTest.Extensions;
using Moq;
using Moq.EntityFrameworkCore;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.QueryHandlers.Product
{
    public class GetProductByIdQueryHandlerTest
    {
        private readonly GetProductByIdQueryHandler _queryHandler;
        private readonly Mock<ProductCatalogDbContext> _mockDbContext;

        public GetProductByIdQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockDbContext = MockProductCatalogDbContextFactory.Create().DbContext;
            _queryHandler = new GetProductByIdQueryHandler(_mockDbContext.Object);
        }

        [Fact]
        public async Task GetProductById_ReturnsProduct()
        {
            // Arrange
            var category = CategoryEntity.Create("Electronics", null, null, 1);
            var product = ProductEntity.Create("Laptop", null, null, category, Money.From(999.99m), 10, 1);

            var products = new List<ProductEntity> { product }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Products).ReturnsDbSet(products);

            // Act
            var result = await _queryHandler.HandleAsync(new GetProductByIdQuery(product.Id), CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(1);
            result.Name.ShouldBe("Laptop");
            result.Price.ShouldBe(999.99m);
        }
    }
}
