using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Implementation.QueryHandlers.Category;
using ECommerceNetApp.Service.Queries.Category;
using Moq;
using Shouldly;
using CategoryEntity = ECommerceNetApp.Domain.Entities.Category;

namespace ECommerceNetApp.Service.UnitTest.QueryHandlers.Category
{
    public class GetCategoryByIdQueryHandlerTest
    {
        private readonly GetCategoryByIdQueryHandler _queryHandler;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;

        public GetCategoryByIdQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _queryHandler = new GetCategoryByIdQueryHandler(_mockCategoryRepository.Object, _mockProductRepository.Object);
        }

        [Fact]
        public async Task GetCategoryById_ReturnsCategory()
        {
            // Arrange
            var category = new CategoryEntity(1, "Test Category");

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(category.Id, CancellationToken.None))
                .ReturnsAsync(category);

            // Act
            var result = await _queryHandler.Handle(new GetCategoryByIdQuery(category.Id), CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("Test Category");
        }
    }
}
