using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.Validators.Product;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace ECommerceNetApp.Service.UnitTest.Validators.Product
{
    public class UpdateProductCommandValidatorTest
    {
        private readonly UpdateProductCommandValidator _validator;
        private readonly Mock<ProductCatalogDbContext> _mockDbContext;

        public UpdateProductCommandValidatorTest()
        {
            var domainEventService = new Mock<IDomainEventService>();
            _mockDbContext = new Mock<ProductCatalogDbContext>(new DbContextOptions<ProductCatalogDbContext>(), domainEventService.Object);
            _validator = new UpdateProductCommandValidator(_mockDbContext.Object);
        }

        [Fact]
        public async Task Validate_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var command = new UpdateProductCommand(1, "Valid Product Name", null, null, 1, 100.0m, null, 10);

            var category = CategoryEntity.Create("Electronics", null, null, 1);
            var product = ProductEntity.Create("Laptop", null, null, category, Money.From(10.0m), 10, 1);
            _mockDbContext.SetupGet(c => c.Products).ReturnsDbSet(new List<ProductEntity>() { product }.AsQueryable());

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyName_ShouldFailValidation()
        {
            // Arrange
            var command = new UpdateProductCommand(1, string.Empty, null, null, 1, 100.0m, null, 10);

            var category = CategoryEntity.Create("Electronics", null, null, 1);
            var product = ProductEntity.Create("Laptop", null, null, category, Money.From(10.0m), 10, 1);
            _mockDbContext.SetupGet(c => c.Products).ReturnsDbSet(new List<ProductEntity> { product }.AsQueryable());

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name)
                .WithErrorMessage("Product name is required.");
        }

        [Fact]
        public async Task Validate_WithNameExceedingMaxLength_ShouldFailValidation()
        {
            // Arrange
            var productName = new string('A', ProductEntity.MaxProductNameLength + 1);
            var command = new UpdateProductCommand(1, productName, null, null, 1, 100.0m, null, 10);

            var category = CategoryEntity.Create("Electronics", null, null, 1);
            var product = ProductEntity.Create("Laptop", null, null, category, Money.From(10.0m), 10, 1);
            _mockDbContext.SetupGet(c => c.Products).ReturnsDbSet(new List<ProductEntity> { product }.AsQueryable());

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name)
                .WithErrorMessage($"Product name cannot exceed {ProductEntity.MaxProductNameLength} characters.");
        }

        [Fact]
        public async Task Validate_WithNegativePrice_ShouldFailValidation()
        {
            // Arrange
            var command = new UpdateProductCommand(1, "Valid Product Name", null, null, 1, -10, null, 10);

            var category = CategoryEntity.Create("Electronics", null, null, 1);
            var product = ProductEntity.Create("Laptop", null, null, category, Money.From(10.0m), 10, 1);
            _mockDbContext.SetupGet(c => c.Products).ReturnsDbSet(new List<ProductEntity> { product }.AsQueryable());

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Price)
                .WithErrorMessage("Product price must be greater than or equal to zero.");
        }

        [Fact]
        public async Task Validate_WithNonPositiveAmount_ShouldFailValidation()
        {
            // Arrange
            var command = new UpdateProductCommand(1, "Valid Product Name", null, null, 1, 100.0m, null, 0);

            var category = CategoryEntity.Create("Electronics", null, null, 1);
            var product = ProductEntity.Create("Laptop", null, null, category, Money.From(10.0m), 10, 1);
            _mockDbContext.SetupGet(c => c.Products).ReturnsDbSet(new List<ProductEntity> { product }.AsQueryable());

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Amount)
                .WithErrorMessage("Product amount must be greater than zero.");
        }

        [Fact]
        public async Task Validate_WithInvalidCategoryId_ShouldFailValidation()
        {
            // Arrange
            var command = new UpdateProductCommand(1, "Valid Product Name", null, null, 0, 100.0m, null, 10);

            var category = CategoryEntity.Create("Electronics", null, null, 1);
            var product = ProductEntity.Create("Laptop", null, null, category, Money.From(10.0m), 10, 1);
            _mockDbContext.SetupGet(c => c.Products).ReturnsDbSet(new List<ProductEntity> { product }.AsQueryable());

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.CategoryId)
                .WithErrorMessage("Product CategoryId must be a valid positive number.");
        }

        [Fact]
        public async Task Validate_WithInvalidProductId_ShouldFailValidation()
        {
            // Arrange
            var command = new UpdateProductCommand(2, "Valid Product Name", null, null, 1, 100.0m, null, 10);

            var category = CategoryEntity.Create("Electronics", null, null, 1);

            var product = ProductEntity.Create("Laptop", null, null, category, Money.From(10.0m), 10, 1);

            _mockDbContext.SetupGet(c => c.Products).ReturnsDbSet(new List<ProductEntity> { product }.AsQueryable());
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(new List<CategoryEntity> { category }.AsQueryable());

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Id)
                .WithErrorMessage("Product does not exist.");
        }
    }
}
