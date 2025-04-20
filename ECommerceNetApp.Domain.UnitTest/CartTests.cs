using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Events.Cart;
using ECommerceNetApp.Domain.Exceptions.Cart;
using ECommerceNetApp.Domain.ValueObjects;
using Shouldly;

namespace ECommerceNetApp.Domain.UnitTest
{
    public class CartTests
    {
        [Fact]
        public void CreateCart_WithValidId_Succeeds()
        {
            // Act
            var cart = new CartEntity("test-cart-123");

            // Assert
            cart.Id.ShouldBe("test-cart-123");
            cart.Items.ShouldBeEmpty();
        }

        [Fact]
        public void CreateCart_WithEmptyId_ThrowsDomainException()
        {
            // Act & Assert
            var exception = Should.Throw<InvalidCartException>(()
                => new CartEntity(string.Empty));
        }

        [Fact]
        public void AddItem_NewItem_RaisesCartItemAddedEvent()
        {
            // Arrange
            var cart = new CartEntity("test-cart");
            var item = new CartItem(1, "Test Item", Money.From(10.99m), 2);

            // Act
            cart.AddItem(item);

            // Assert
            cart.DomainEvents.ShouldContain(e => e is CartItemAddedEvent);
        }

        [Fact]
        public void AddItem_NewItem_AddsToCart()
        {
            // Arrange
            var cart = new CartEntity("test-cart-123");
            var item = new CartItem(1, "Test Item", Money.From(10.99m), 2);

            // Act
            cart.AddItem(item);

            // Assert
            cart.Items.ShouldContain(i => i.Name == "Test Item" && i.Quantity == 2, 1);

            // Verify domain event was raised
            cart.DomainEvents.ShouldContain(x => x is CartItemAddedEvent, 1);
        }

        [Fact]
        public void AddItem_ExistingItem_UpdatesQuantity()
        {
            // Arrange
            var cart = new CartEntity("test-cart-123");
            var item1 = new CartItem(1, "Test Item", Money.From(10.99m), 2);
            var item2 = new CartItem(1, "Test Item", Money.From(10.99m), 3);

            // Act
            cart.AddItem(item1);
            cart.AddItem(item2);

            // Assert
            cart.Items.ShouldContain(c => c.Quantity == 5);

            // Verify domain events
            cart.DomainEvents.Count.ShouldBe(2);
            cart.DomainEvents.ShouldContain(x => x is CartItemAddedEvent, 1);
            cart.DomainEvents.ShouldContain(x => x is CartItemQuantityUpdatedEvent, 1);
        }

        [Fact]
        public void RemoveItem_ExistingItem_RemovesFromCart()
        {
            // Arrange
            var cart = new CartEntity("test-cart-123");
            var item = new CartItem(1, "Test Item", Money.From(10.99m), 2);
            cart.AddItem(item);
            cart.ClearDomainEvents(); // Clear add event for this test

            // Act
            cart.RemoveItem(1);

            // Assert
            cart.Items.ShouldBeEmpty();

            // Verify domain event
            cart.DomainEvents.Count.ShouldBe(1);
            cart.DomainEvents.ShouldContain(x => x is CartItemRemovedEvent, 1);
        }

        [Fact]
        public void RemoveItem_NonExistingItem_ThrowsException()
        {
            // Arrange
            var cart = new CartEntity("test-cart-123");

            // Act & Assert
            var exception = Should.Throw<InvalidCartException>(() => cart.RemoveItem(1));
        }

        [Fact]
        public void UpdateItemQuantity_ExistingItem_UpdatesQuantity()
        {
            // Arrange
            var cart = new CartEntity("test-cart-123");
            var item = new CartItem(1, "Test Item", Money.From(10.99m), 2);
            cart.AddItem(item);
            cart.ClearDomainEvents(); // Clear add event for this test

            // Act
            cart.UpdateItemQuantity(1, 5);

            // Assert
            cart.Items.ShouldContain(c => c.Quantity == 5);

            // Verify domain event
            cart.DomainEvents.Count.ShouldBe(1);
            cart.DomainEvents.ShouldContain(x => x is CartItemQuantityUpdatedEvent, 1);
            var updateEvent = cart.DomainEvents.OfType<CartItemQuantityUpdatedEvent>().Single();
            updateEvent.OldQuantity.ShouldBe(2);
            updateEvent.NewQuantity.ShouldBe(5);
        }

        [Fact]
        public void CalculateTotal_WithMultipleItems_ReturnsTotalPrice()
        {
            // Arrange
            var cart = new CartEntity("test-cart-123");
            var item1 = new CartItem(1, "Item 1", Money.From(10.00m), 2);
            var item2 = new CartItem(2, "Item 2", Money.From(5.50m), 3);

            cart.AddItem(item1);
            cart.AddItem(item2);

            // Act
            var total = cart.CalculateTotal();

            // Assert
            total.Amount.ShouldBe(36.50m); // (10.00 * 2) + (5.50 * 3)
        }
    }
}