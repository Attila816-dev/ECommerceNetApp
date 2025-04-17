using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.Validators.Product;
using FluentValidation.TestHelper;

namespace ECommerceNetApp.Service.UnitTest.Validators.Product
{
    public class UpdateProductCommandValidatorTest
    {
        private readonly UpdateProductCommandValidator _validator;

        public UpdateProductCommandValidatorTest()
        {
            _validator = new UpdateProductCommandValidator();
        }

        [Fact]
        public void Validate_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var command = new UpdateProductCommand(1, "Valid Product Name", null, null, 1, 100.0m, 10);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithEmptyName_ShouldFailValidation()
        {
            // Arrange
            var command = new UpdateProductCommand(1, string.Empty, null, null, 1, 100.0m, 10);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name)
                .WithErrorMessage("Product name is required.");
        }

        [Fact]
        public void Validate_WithNameExceedingMaxLength_ShouldFailValidation()
        {
            // Arrange
            var productName = new string('A', ProductEntity.MaxProductNameLength + 1);
            var command = new UpdateProductCommand(1, productName, null, null, 1, 100.0m, 10);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name)
                .WithErrorMessage($"Product name cannot exceed {ProductEntity.MaxProductNameLength} characters.");
        }

        [Fact]
        public void Validate_WithNonPositivePrice_ShouldFailValidation()
        {
            // Arrange
            var command = new UpdateProductCommand(1, "Valid Product Name", null, null, 1, 0, 10);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Price)
                .WithErrorMessage("Product price must be greater than zero.");
        }

        [Fact]
        public void Validate_WithNonPositiveAmount_ShouldFailValidation()
        {
            // Arrange
            var command = new UpdateProductCommand(1, "Valid Product Name", null, null, 1, 100.0m, 0);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Amount)
                .WithErrorMessage("Product amount must be greater than zero.");
        }

        [Fact]
        public void Validate_WithInvalidCategoryId_ShouldFailValidation()
        {
            // Arrange
            var command = new UpdateProductCommand(1, "Valid Product Name", null, null, 0, 100.0m, 10);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.CategoryId)
                .WithErrorMessage("Category ID must be a valid positive number.");
        }
    }
}
