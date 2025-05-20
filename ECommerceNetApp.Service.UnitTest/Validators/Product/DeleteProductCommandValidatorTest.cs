using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.UnitTest.Extensions;
using ECommerceNetApp.Service.Validators.Product;
using FluentValidation.TestHelper;
using Moq;
using Moq.EntityFrameworkCore;

namespace ECommerceNetApp.Service.UnitTest.Validators.Product
{
    public class DeleteProductCommandValidatorTest
    {
        private readonly DeleteProductCommandValidator _validator;
        private readonly Mock<ProductCatalogDbContext> _mockDbContext;

        public DeleteProductCommandValidatorTest()
        {
            _mockDbContext = MockProductCatalogDbContextFactory.Create().DbContext;
            _validator = new DeleteProductCommandValidator(_mockDbContext.Object);
        }

        [Fact]
        public async Task DeleteProductAsync_WithNonExistingCategory_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var categoryId = 2;

            var category = CategoryEntity.Create("Electronics", null, null, 1);
            var product = ProductEntity.Create("Laptop", null, null, category, Money.From(10.0m), 10, 1);

            _mockDbContext.SetupGet(c => c.Products).ReturnsDbSet(new List<ProductEntity> { product }.AsQueryable());
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(new List<CategoryEntity> { category }.AsQueryable());

            var command = new DeleteProductCommand(categoryId);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Id)
                .WithErrorMessage("Product does not exist.");
        }
    }
}
