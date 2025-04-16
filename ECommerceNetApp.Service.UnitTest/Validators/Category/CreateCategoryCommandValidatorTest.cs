using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.Validators.Category;
using FluentValidation.TestHelper;

namespace ECommerceNetApp.Service.UnitTest.Validators.Category
{
    public class CreateCategoryCommandValidatorTest
    {
        private readonly CreateCategoryCommandValidator _validator;

        public CreateCategoryCommandValidatorTest()
        {
            _validator = new CreateCategoryCommandValidator();
        }

        [Fact]
        public void Validate_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var command = new CreateCategoryCommand("Valid Category Name", null, null);

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

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name)
                .WithErrorMessage($"Category name cannot exceed {CategoryEntity.MaxCategoryNameLength} characters.");
        }

        [Fact]
        public void Validate_WithInvalidParentCategoryId_ShouldFailValidation()
        {
            // Arrange
            var command = new CreateCategoryCommand("Valid Category Name", null, 0);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.ParentCategoryId)
                .WithErrorMessage("ParentCategory ID must be a valid positive number.");
        }

        [Fact]
        public void Validate_WithNullParentCategoryId_ShouldPassValidation()
        {
            // Arrange
            var command = new CreateCategoryCommand("Valid Category Name", null, null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.ParentCategoryId);
        }
    }
}
