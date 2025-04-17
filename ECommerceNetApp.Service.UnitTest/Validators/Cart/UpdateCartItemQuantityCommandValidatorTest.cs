using ECommerceNetApp.Service.Commands.Cart;
using ECommerceNetApp.Service.Validators.Cart;
using FluentValidation.TestHelper;

namespace ECommerceNetApp.Service.UnitTest.Validators.Category
{
    public class UpdateCartItemQuantityCommandValidatorTest
    {
        private readonly UpdateCartItemQuantityCommandValidator _validator;

        public UpdateCartItemQuantityCommandValidatorTest()
        {
            _validator = new UpdateCartItemQuantityCommandValidator();
        }

        [Fact]
        public void Validate_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var command = new UpdateCartItemQuantityCommand("Valid Cart", 1, 2);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void UpdateCartItemQuantityCommandValidationWithInvalidCartId_ShouldReturnErrorMessage()
        {
            // Act
            var result = _validator.TestValidate(new UpdateCartItemQuantityCommand(string.Empty, 1, 2));

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.CartId)
                .WithErrorMessage("Cart Id is required.");
        }

        [Fact]
        public void UpdateCartItemQuantityCommandValidationWithInvalidQuantity_ShouldReturnErrorMessage()
        {
            // Arrange
            string testCartId = "test-cart-123";

            // Act
            var result = _validator.TestValidate(new UpdateCartItemQuantityCommand(testCartId, 1, -1));

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Quantity)
                .WithErrorMessage("Cart item quantity must be greater than zero.");
        }
    }
}
