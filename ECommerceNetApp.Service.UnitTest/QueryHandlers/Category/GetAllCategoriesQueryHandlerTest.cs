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
    public class GetAllCategoriesQueryHandlerTest
    {
        private readonly GetAllCategoriesQueryHandler _queryHandler;
        private readonly Mock<ProductCatalogDbContext> _mockDbContext;

        public GetAllCategoriesQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            var mockDomainEventService = new Mock<IDomainEventService>();
            _mockDbContext = new Mock<ProductCatalogDbContext>(new DbContextOptions<ProductCatalogDbContext>(), mockDomainEventService.Object);
            _queryHandler = new GetAllCategoriesQueryHandler(_mockDbContext.Object);
        }

        [Fact]
        public async Task GetAllCategories_ReturnsCategories()
        {
            // Arrange
            var categories = new List<CategoryEntity>
            {
                CategoryEntity.Create("Electronics", null, null, 1),
                CategoryEntity.Create("Books", null, null, 2),
            }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            // Act
            var result = await _queryHandler.HandleAsync(new GetAllCategoriesQuery(), CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Count().ShouldBe(2);
            result.ShouldContain(c => c.Id == 1 && c.Name == "Electronics");
            result.ShouldContain(c => c.Id == 2 && c.Name == "Books");
        }
    }
}
