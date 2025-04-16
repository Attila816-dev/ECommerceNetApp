using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Implementation.QueryHandlers.Category;
using ECommerceNetApp.Service.Queries.Category;
using Moq;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.QueryHandlers.Category
{
    public class GetCategoriesByParentCategoryIdQueryHandlerTest
    {
        private readonly GetCategoriesByParentCategoryIdQueryHandler _queryHandler;
        private readonly Mock<ICategoryRepository> _mockRepository;

        public GetCategoriesByParentCategoryIdQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<ICategoryRepository>();
            _queryHandler = new GetCategoriesByParentCategoryIdQueryHandler(_mockRepository.Object);
        }

        [Fact]
        public async Task GetCategoryByParentCategoryId_ReturnsCategory()
        {
            // Arrange
            var category = new CategoryEntity(1, "Test Category");

            _mockRepository
                .Setup(r => r.GetByParentCategoryIdAsync(category.Id, CancellationToken.None))
                .ReturnsAsync([category]);

            // Act
            var result = await _queryHandler.Handle(new GetCategoriesByParentCategoryIdQuery(category.Id), CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Count().ShouldBe(1);
            result.First().Name.ShouldBe("Test Category");
        }
    }
}
