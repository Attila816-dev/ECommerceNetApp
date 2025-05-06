using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Implementation.Mappers.Category;
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
        private readonly Mock<IProductCatalogUnitOfWork> _mockUnitOfWork;
        private readonly CategoryMapper _categoryMapper;

        public GetCategoriesByParentCategoryIdQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<ICategoryRepository>();
            _mockUnitOfWork = new Mock<IProductCatalogUnitOfWork>();
            _mockUnitOfWork.SetupGet(u => u.CategoryRepository).Returns(_mockRepository.Object);
            _categoryMapper = new CategoryMapper();
            _queryHandler = new GetCategoriesByParentCategoryIdQueryHandler(_mockUnitOfWork.Object, _categoryMapper);
        }

        [Fact]
        public async Task GetCategoryByParentCategoryId_ReturnsCategory()
        {
            // Arrange
            var category = CategoryEntity.Create("Test Category", null, null, 1);

            _mockRepository
                .Setup(r => r.GetByParentCategoryIdAsync(category.Id, CancellationToken.None))
                .ReturnsAsync([category]);

            // Act
            var result = await _queryHandler.HandleAsync(new GetCategoriesByParentCategoryIdQuery(category.Id), CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Count().ShouldBe(1);
            result.ShouldContain(c => c.Name == "Test Category");
        }
    }
}
