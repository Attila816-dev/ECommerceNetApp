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
    public class GetAllProductsQueryHandlerTest
    {
        private readonly GetAllProductsQueryHandler _queryHandler;
        private readonly Mock<IProductRepository> _mockRepository;
        private readonly Mock<IProductCatalogUnitOfWork> _mockUnitOfWork;
        private readonly ProductMapper _productMapper;

        public GetAllProductsQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<IProductRepository>();
            _mockUnitOfWork = new Mock<IProductCatalogUnitOfWork>();
            _mockUnitOfWork.SetupGet(x => x.ProductRepository).Returns(_mockRepository.Object);
            _productMapper = new ProductMapper();
            _queryHandler = new GetAllProductsQueryHandler(_mockUnitOfWork.Object, _productMapper);
        }

        [Fact]
        public async Task GetAllCategories_ReturnsCategories()
        {
            var category = CategoryEntity.Create("Electronics", null, null, 1);

            // Arrange
            var products = new List<ProductEntity>
            {
                ProductEntity.Create("Laptop", null, null, category, new Money(999.99m), 10, 1),
                ProductEntity.Create("Smartphone", null, null, category, new Money(499.99m), 20, 2),
            };
            _mockRepository
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync(products);

            // Act
            var result = await _queryHandler.Handle(new GetAllProductsQuery(), CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Count().ShouldBe(2);
            result.ShouldContain(c => c.Id == 1 && c.Name == "Laptop");
            result.ShouldContain(c => c.Id == 2 && c.Name == "Smartphone");
        }
    }
}
