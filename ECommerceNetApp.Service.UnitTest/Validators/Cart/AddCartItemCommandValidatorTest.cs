using ECommerceNetApp.Service.Commands.Cart;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Validators.Cart;
using FluentValidation.TestHelper;

namespace ECommerceNetApp.Service.UnitTest.Validators.Category
{
    public class AddCartItemCommandValidatorTest
    {
        private readonly AddCartItemCommandValidator _validator;

        public AddCartItemCommandValidatorTest()
        {
            _validator = new AddCartItemCommandValidator();
        }

        [Fact]
        public void Validate_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var command = new AddCartItemCommand(
                "Valid Cart",
                new CartItemDto
                {
                    Currency = "EUR",
                    Id = 1,
                    Name = "Valid Item",
                    Price = 10.99m,
                    Quantity = 2,
                });

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void AddCartItemCommandValidationWithoutItem_ShouldReturnErrorMessage()
        {
            // Arrange
            string testCartId = "test-cart-123";

            // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var result = _validator.TestValidate(new AddCartItemCommand(testCartId, default));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Item)
                .WithErrorMessage("Cart item cannot be null.");
        }

        [Fact]
        public void AddCartItemCommandValidationWithoutName_ShouldReturnErrorMessage()
        {
            // Arrange
            string testCartId = "test-cart-123";

            var cartItemDto = new CartItemDto
            {
                Id = 1,
                Price = 15.99m,
                Quantity = 1,
            };

            // Act
            var result = _validator.TestValidate(new AddCartItemCommand(testCartId, cartItemDto));

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Item.Name)
                .WithErrorMessage("Cart item name is required.");
        }

        [Fact]
        public void AddCartItemCommandValidationWithInvalidItem_ShouldReturnErrorMessage()
        {
            // Arrange
            string testCartId = "test-cart-123";
            var cartItemDto = new CartItemDto
            {
                Id = 0, // Invalid ID
                Name = "Test Item",
                Price = 10.99m,
                Quantity = 1,
            };

            // Act
            var result = _validator.TestValidate(new AddCartItemCommand(testCartId, cartItemDto));

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Item.Id)
                .WithErrorMessage("Cart item Id must be a positive number.");
        }

        [Fact]
        public void AddCartItemCommandValidationWithInvalidQuantity_ShouldReturnErrorMessage()
        {
            // Arrange
            string testCartId = "test-cart-123";
            var cartItemDto = new CartItemDto
            {
                Id = 2, // Invalid ID
                Name = "Test Item",
                Price = 10.99m,
                Quantity = -1,
            };

            // Act
            var result = _validator.TestValidate(new AddCartItemCommand(testCartId, cartItemDto));

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Item.Quantity)
                .WithErrorMessage("Cart item quantity must be greater than zero.");
        }

        [Fact]
        public void AddCartItemCommandValidationWithInvalidPrice_ShouldReturnErrorMessage()
        {
            // Arrange
            string testCartId = "test-cart-123";
            var cartItemDto = new CartItemDto
            {
                Id = 2, // Invalid ID
                Name = "Test Item",
                Price = -10.99m,
                Quantity = 1,
            };

            // Act
            var result = _validator.TestValidate(new AddCartItemCommand(testCartId, cartItemDto));

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Item.Price)
                .WithErrorMessage("Cart item price must be greater than zero.");
        }
    }
}
