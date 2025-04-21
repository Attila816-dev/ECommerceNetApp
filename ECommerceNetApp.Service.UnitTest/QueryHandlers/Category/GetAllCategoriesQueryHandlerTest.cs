using System.Linq.Expressions;
using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Implementation.Mappers.Category;
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
        private readonly Mock<IProductCatalogUnitOfWork> _mockUnitOfWork;
        private readonly CategoryMapper _categoryMapper;

        public GetAllCategoriesQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<ICategoryRepository>();
            _mockUnitOfWork = new Mock<IProductCatalogUnitOfWork>();
            _mockUnitOfWork.SetupGet(u => u.CategoryRepository).Returns(_mockRepository.Object);

            _categoryMapper = new CategoryMapper();
            _queryHandler = new GetAllCategoriesQueryHandler(_mockUnitOfWork.Object, _categoryMapper);
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
                .Setup(r => r.GetAllAsync(
                    It.IsAny<Expression<Func<CategoryEntity, bool>>?>(),
                    It.IsAny<Func<IQueryable<CategoryEntity>, IQueryable<CategoryEntity>>?>(),
                    CancellationToken.None))
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
