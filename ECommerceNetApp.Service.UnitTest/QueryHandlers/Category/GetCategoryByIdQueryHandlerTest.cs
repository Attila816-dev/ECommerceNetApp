using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Implementation.Mappers.Category;
using ECommerceNetApp.Service.Implementation.QueryHandlers.Category;
using ECommerceNetApp.Service.Queries.Category;
using Moq;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.QueryHandlers.Category
{
    public class GetCategoryByIdQueryHandlerTest
    {
        private readonly GetCategoryByIdQueryHandler _queryHandler;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IProductCatalogUnitOfWork> _mockUnitOfWork;
        private readonly CategoryMapper _categoryMapper;

        public GetCategoryByIdQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUnitOfWork = new Mock<IProductCatalogUnitOfWork>();
            _mockUnitOfWork.SetupGet(u => u.CategoryRepository).Returns(_mockCategoryRepository.Object);
            _mockUnitOfWork.SetupGet(u => u.ProductRepository).Returns(_mockProductRepository.Object);

            _categoryMapper = new CategoryMapper();

            _queryHandler = new GetCategoryByIdQueryHandler(_mockUnitOfWork.Object, _categoryMapper);
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
