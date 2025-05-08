using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Implementation.QueryHandlers.Category;
using ECommerceNetApp.Service.Queries.Category;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.QueryHandlers.Category
{
    public class GetCategoriesByParentCategoryIdQueryHandlerTest
    {
        private readonly GetCategoriesByParentCategoryIdQueryHandler _queryHandler;
        private readonly Mock<ProductCatalogDbContext> _mockDbContext;

        public GetCategoriesByParentCategoryIdQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            var mockDomainEventService = new Mock<IDomainEventService>();
            _mockDbContext = new Mock<ProductCatalogDbContext>(new DbContextOptions<ProductCatalogDbContext>(), mockDomainEventService.Object);
            _queryHandler = new GetCategoriesByParentCategoryIdQueryHandler(_mockDbContext.Object);
        }

        [Fact]
        public async Task GetCategoryByParentCategoryId_ReturnsCategory()
        {
            // Arrange
            var parentCategory = CategoryEntity.Create("Test Category", null, null, 1);
            var category = CategoryEntity.Create("Test Category", null, parentCategory, 2);
            var categories = new List<CategoryEntity> { category, parentCategory }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            // Act
            var result = await _queryHandler.HandleAsync(new GetCategoriesByParentCategoryIdQuery(parentCategory.Id), CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Count().ShouldBe(1);
            result.ShouldContain(c => c.Name == "Test Category");
        }
    }
}
