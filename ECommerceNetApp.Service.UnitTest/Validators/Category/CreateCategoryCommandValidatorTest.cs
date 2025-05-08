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
    public class CreateCategoryCommandValidatorTest
    {
        private readonly CreateCategoryCommandValidator _validator;
        private readonly Mock<ProductCatalogDbContext> _mockDbContext;

        public CreateCategoryCommandValidatorTest()
        {
            var domainEventService = new Mock<IDomainEventService>();
            _mockDbContext = new Mock<ProductCatalogDbContext>(new DbContextOptions<ProductCatalogDbContext>(), domainEventService.Object);
            _validator = new CreateCategoryCommandValidator(_mockDbContext.Object);
        }

        [Fact]
        public void Validate_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var command = new CreateCategoryCommand("Valid Category Name", null, null);

            var category = CategoryEntity.Create("test-category", null, null, 1);

            var categories = new List<CategoryEntity> { category }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithEmptyName_ShouldFailValidation()
        {
            // Arrange
            var command = new CreateCategoryCommand(string.Empty, null, null);

            var category = CategoryEntity.Create("test-category", null, null, 1);

            var categories = new List<CategoryEntity> { category }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name)
                .WithErrorMessage("Category name is required.");
        }

        [Fact]
        public void Validate_WithNameExceedingMaxLength_ShouldFailValidation()
        {
            // Arrange
            var categoryName = new string('A', CategoryEntity.MaxCategoryNameLength + 1);
            var command = new CreateCategoryCommand(categoryName, null, null);

            var category = CategoryEntity.Create("test-category", null, null, 1);

            var categories = new List<CategoryEntity> { category }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name)
                .WithErrorMessage($"Category name cannot exceed {CategoryEntity.MaxCategoryNameLength} characters.");
        }

        [Fact]
        public async Task Validate_WithInvalidParentCategoryId_ShouldFailValidation()
        {
            // Arrange
            var command = new CreateCategoryCommand("Valid Category Name", null, 2);

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
        public void Validate_WithNullParentCategoryId_ShouldPassValidation()
        {
            // Arrange
            var command = new CreateCategoryCommand("Valid Category Name", null, null);

            var category = CategoryEntity.Create("test-category", null, null, 1);

            var categories = new List<CategoryEntity> { category }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.ParentCategoryId);
        }
    }
}
