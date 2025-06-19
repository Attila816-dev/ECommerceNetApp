using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.Implementation.Validators.Category;
using ECommerceNetApp.Service.UnitTest.Extensions;
using FluentValidation.TestHelper;
using Moq;
using Moq.EntityFrameworkCore;

namespace ECommerceNetApp.Service.UnitTest.Validators.Category
{
    public class UpdateCategoryCommandValidatorTest
    {
        private readonly UpdateCategoryCommandValidator _validator;
        private readonly Mock<ProductCatalogDbContext> _mockDbContext;

        public UpdateCategoryCommandValidatorTest()
        {
            _mockDbContext = MockProductCatalogDbContextFactory.Create().DbContext;
            _validator = new UpdateCategoryCommandValidator(_mockDbContext.Object);
        }

        [Fact]
        public async Task Validate_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var command = new UpdateCategoryCommand(1, "Valid Category Name", null, null);

            var category = CategoryEntity.Create("test-category", null, null, 1);

            var categories = new List<CategoryEntity> { category }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyName_ShouldFailValidation()
        {
            // Arrange
            var command = new UpdateCategoryCommand(1, string.Empty, null, null);
            var category = CategoryEntity.Create("test-category", null, null, 1);

            var categories = new List<CategoryEntity> { category }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name)
                .WithErrorMessage("Category name is required.");
        }

        [Fact]
        public async Task Validate_WithNameExceedingMaxLength_ShouldFailValidation()
        {
            // Arrange
            var categoryName = new string('A', CategoryEntity.MaxCategoryNameLength + 1);
            var command = new UpdateCategoryCommand(1, categoryName, null, null);

            var category = CategoryEntity.Create("test-category", null, null, 1);

            var categories = new List<CategoryEntity> { category }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name)
                .WithErrorMessage($"Category Name cannot exceed {CategoryEntity.MaxCategoryNameLength} characters.");
        }

        [Fact]
        public async Task Validate_WithInvalidParentCategoryId_ShouldFailValidation()
        {
            // Arrange
            var command = new UpdateCategoryCommand(1, "Valid Category Name", null, 0);

            var category = CategoryEntity.Create("test-category", null, null, 1);

            var categories = new List<CategoryEntity> { category }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.ParentCategoryId)
                .WithErrorMessage("Parent Category does not exist.");
        }

        [Fact]
        public async Task Validate_WithInvalidCategoryId_ShouldFailValidation()
        {
            // Arrange
            var command = new UpdateCategoryCommand(1, "Valid Category Name", null, null);

            var category = CategoryEntity.Create("test-category", null, null, 2);

            var categories = new List<CategoryEntity> { category }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Id)
                .WithErrorMessage("Category does not exist.");
        }

        [Fact]
        public async Task Validate_WithNullParentCategoryId_ShouldPassValidation()
        {
            // Arrange
            var command = new UpdateCategoryCommand(1, "Valid Category Name", null, null);
            var category = CategoryEntity.Create("test-category", null, null, 1);

            var categories = new List<CategoryEntity> { category }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.ParentCategoryId);
        }
    }
}
