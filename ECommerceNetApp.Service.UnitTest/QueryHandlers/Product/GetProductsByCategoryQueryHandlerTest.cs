using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Implementation.Mappers.Product;
using ECommerceNetApp.Service.Implementation.QueryHandlers.Product;
using ECommerceNetApp.Service.Queries.Product;
using Moq;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.QueryHandlers.Category
{
    public class GetProductsByCategoryQueryHandlerTest
    {
        private readonly GetProductsByCategoryQueryHandler _queryHandler;
        private readonly Mock<IProductCatalogUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IProductRepository> _mockRepository;
        private readonly ProductMapper _productMapper;

        public GetProductsByCategoryQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<IProductRepository>();
            _mockUnitOfWork = new Mock<IProductCatalogUnitOfWork>();
            _mockUnitOfWork.Setup(u => u.ProductRepository).Returns(_mockRepository.Object);

            _productMapper = new ProductMapper();
            _queryHandler = new GetProductsByCategoryQueryHandler(_mockUnitOfWork.Object, _productMapper);
        }

        [Fact]
        public async Task GetProductsByCategoryIdAsync_ShouldReturnProductsInCategory()
        {
            // Arrange
            var category = CategoryEntity.Create("Electronics", null, null, 1);
            var products = new List<ProductEntity>
            {
                ProductEntity.Create("Laptop", null, null, category, Money.From(999.99m), 10, 1),
                ProductEntity.Create("Smartphone", null, null, category, Money.From(499.99m), 20, 2),
            };

            _mockRepository.Setup(repo => repo.GetProductsByCategoryIdAsync(category.Id, CancellationToken.None))
                .ReturnsAsync(products);

            // Act
            var result = await _queryHandler.Handle(new GetProductsByCategoryQuery(category.Id), CancellationToken.None);

            // Assert
            result.Count().ShouldBe(2);
            result.ShouldAllBe(p => p.CategoryId == category.Id);
            result.ShouldContain(c => c.Id == 1 && c.Name == "Laptop");
            result.ShouldContain(c => c.Id == 2 && c.Name == "Smartphone");
        }
    }
}
