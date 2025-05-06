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
    public class GetProductByIdQueryHandlerTest
    {
        private readonly GetProductByIdQueryHandler _queryHandler;
        private readonly Mock<IProductRepository> _mockRepository;
        private readonly Mock<IProductCatalogUnitOfWork> _mockUnitOfWork;
        private readonly ProductMapper _productMapper;

        public GetProductByIdQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<IProductRepository>();
            _mockUnitOfWork = new Mock<IProductCatalogUnitOfWork>();
            _mockUnitOfWork.SetupGet(x => x.ProductRepository).Returns(_mockRepository.Object);
            _productMapper = new ProductMapper();
            _queryHandler = new GetProductByIdQueryHandler(_mockUnitOfWork.Object, _productMapper);
        }

        [Fact]
        public async Task GetProductById_ReturnsProduct()
        {
            // Arrange
            var category = CategoryEntity.Create("Electronics", null, null, 1);
            var product = ProductEntity.Create("Laptop", null, null, category, Money.From(999.99m), 10, 1);

            _mockRepository
                .Setup(r => r.GetByIdAsync(product.Id, CancellationToken.None))
                .ReturnsAsync(product);

            // Act
            var result = await _queryHandler.Handle(new GetProductByIdQuery(product.Id), CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(1);
            result.Name.ShouldBe("Laptop");
            result.Price.ShouldBe(999.99m);
        }
    }
}
