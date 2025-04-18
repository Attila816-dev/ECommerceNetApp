using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
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
        private readonly ProductMapper _productMapper;

        public GetAllProductsQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<IProductRepository>();
            _productMapper = new ProductMapper();
            _queryHandler = new GetAllProductsQueryHandler(_mockRepository.Object, _productMapper);
        }

        [Fact]
        public async Task GetAllCategories_ReturnsCategories()
        {
            var category = new CategoryEntity(1, "Electronics");

            // Arrange
            var products = new List<ProductEntity>
            {
                new ProductEntity(1, "Laptop", null, null, category, 999.99m, 10),
                new ProductEntity(2, "Smartphone", null, null, category, 499.99m, 20),
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
