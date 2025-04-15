using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Implementation.QueryHandlers.Category;
using ECommerceNetApp.Service.Implementation.QueryHandlers.Product;
using ECommerceNetApp.Service.Queries.Category;
using ECommerceNetApp.Service.Queries.Product;
using Moq;
using Shouldly;
using CategoryEntity = ECommerceNetApp.Domain.Entities.Category;
using ProductEntity = ECommerceNetApp.Domain.Entities.Product;

namespace ECommerceNetApp.Service.UnitTest.QueryHandlers.Category
{
    public class GetProductByIdQueryHandlerTest
    {
        private readonly GetProductByIdQueryHandler _queryHandler;
        private readonly Mock<IProductRepository> _mockRepository;

        public GetProductByIdQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<IProductRepository>();
            _queryHandler = new GetProductByIdQueryHandler(_mockRepository.Object);
        }

        [Fact]
        public async Task GetProductById_ReturnsProduct()
        {
            // Arrange
            var category = new CategoryEntity(1, "Electronics");
            var product = new ProductEntity(1, "Laptop", null, null, category, 999.99m, 10);

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
