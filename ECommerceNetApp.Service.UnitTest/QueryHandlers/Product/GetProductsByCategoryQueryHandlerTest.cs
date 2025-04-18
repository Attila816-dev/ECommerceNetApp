﻿using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Implementation.QueryHandlers.Product;
using ECommerceNetApp.Service.Queries.Product;
using Moq;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.QueryHandlers.Category
{
    public class GetProductsByCategoryQueryHandlerTest
    {
        private readonly GetProductsByCategoryQueryHandler _queryHandler;
        private readonly Mock<IProductRepository> _mockRepository;

        public GetProductsByCategoryQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<IProductRepository>();
            _queryHandler = new GetProductsByCategoryQueryHandler(_mockRepository.Object);
        }

        [Fact]
        public async Task GetProductsByCategoryIdAsync_ShouldReturnProductsInCategory()
        {
            // Arrange
            var category = new CategoryEntity(1, "Electronics");
            var products = new List<ProductEntity>
            {
                new ProductEntity(1, "Laptop", null, null, category, 999.99m, 10),
                new ProductEntity(2, "Smartphone", null, null, category, 499.99m, 20),
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
