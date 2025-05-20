using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Implementation.QueryHandlers.Category;
using ECommerceNetApp.Service.Queries.Category;
using ECommerceNetApp.Service.UnitTest.Extensions;
using Moq;
using Moq.EntityFrameworkCore;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.QueryHandlers.Category
{
    public class GetCategoryByIdQueryHandlerTest
    {
        private readonly GetCategoryByIdQueryHandler _queryHandler;
        private readonly Mock<ProductCatalogDbContext> _mockDbContext;

        public GetCategoryByIdQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockDbContext = MockProductCatalogDbContextFactory.Create().DbContext;
            _queryHandler = new GetCategoryByIdQueryHandler(_mockDbContext.Object);
        }

        [Fact]
        public async Task GetCategoryById_ReturnsCategory()
        {
            // Arrange
            var category = CategoryEntity.Create("Test Category", null, null, 1);

            var categories = new List<CategoryEntity> { category }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            // Act
            var result = await _queryHandler.HandleAsync(new GetCategoryByIdQuery(category.Id), CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("Test Category");
        }
    }
}
