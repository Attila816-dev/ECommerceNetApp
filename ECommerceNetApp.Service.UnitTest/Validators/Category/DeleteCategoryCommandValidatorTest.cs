using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.Validators.Category;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace ECommerceNetApp.Service.UnitTest.Validators.Category
{
    public class DeleteCategoryCommandValidatorTest
    {
        private readonly DeleteCategoryCommandValidator _validator;
        private readonly Mock<ProductCatalogDbContext> _mockDbContext;
        private readonly Mock<IEventBus> _mockEventBus;

        public DeleteCategoryCommandValidatorTest()
        {
            _mockEventBus = new Mock<IEventBus>();
            _mockDbContext = new Mock<ProductCatalogDbContext>(new DbContextOptions<ProductCatalogDbContext>(), _mockEventBus.Object);
            _validator = new DeleteCategoryCommandValidator(_mockDbContext.Object);
        }

        [Fact]
        public async Task DeleteCategoryAsync_WithNonExistingCategory_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var categoryId = 2;
            var category = CategoryEntity.Create("test-category", null, null, 1);

            var categories = new List<CategoryEntity> { category }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            var command = new DeleteCategoryCommand(categoryId);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Id)
                .WithErrorMessage("Category does not exist.");
        }
    }
}
