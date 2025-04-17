using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Implementation.QueryHandlers.Category;
using ECommerceNetApp.Service.Queries.Category;
using Moq;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.QueryHandlers.Category
{
    public class GetAllCategoriesQueryHandlerTest
    {
        private readonly GetAllCategoriesQueryHandler _queryHandler;
        private readonly Mock<ICategoryRepository> _mockRepository;

        public GetAllCategoriesQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<ICategoryRepository>();
            _queryHandler = new GetAllCategoriesQueryHandler(_mockRepository.Object);
        }

        [Fact]
        public async Task GetAllCategories_ReturnsCategories()
        {
            // Arrange
            var categories = new List<CategoryEntity>
            {
                new CategoryEntity(1, "Electronics"),
                new CategoryEntity(2, "Books"),
            };
            _mockRepository
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync(categories);

            // Act
            var result = await _queryHandler.Handle(new GetAllCategoriesQuery(), CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Count().ShouldBe(2);
            result.ShouldContain(c => c.Id == 1 && c.Name == "Electronics");
            result.ShouldContain(c => c.Id == 2 && c.Name == "Books");
        }
    }
}
