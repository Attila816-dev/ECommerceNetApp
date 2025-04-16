using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Events.Cart;
using ECommerceNetApp.Domain.Exceptions;
using ECommerceNetApp.Domain.ValueObjects;
using FluentAssertions;

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
            cart.Id.Should().Be("test-cart-123", cart.Id);
            cart.Items.Should().BeEmpty();
        }

        [Fact]
        public void CreateCart_WithEmptyId_ThrowsDomainException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new CartEntity(string.Empty));
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
            cart.Items.Should().ContainSingle()
                .Which.Should().Match<CartItem>(i => i.Name == "Test Item" && i.Quantity == 2);

            // Verify domain event was raised
            cart.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<CartItemAddedEvent>();
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
            cart.Items.Should().ContainSingle()
                .Which.Quantity.Should().Be(5);

            // Verify domain events
            cart.DomainEvents.Should().HaveCount(2);
            cart.DomainEvents.First().Should().BeOfType<CartItemAddedEvent>();
            cart.DomainEvents.Skip(1).First().Should().BeOfType<CartItemQuantityUpdatedEvent>();
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
            cart.Items.Should().BeEmpty();

            // Verify domain event
            cart.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<CartItemRemovedEvent>();
        }

        [Fact]
        public void RemoveItem_NonExistingItem_ThrowsException()
        {
            // Arrange
            var cart = new CartEntity("test-cart-123");

            // Act & Assert
            var exception = Assert.Throws<CartItemNotFoundException>(() => cart.RemoveItem(1));
            exception.Should().NotBeNull();
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
            cart.Items.First().Quantity.Should().Be(5);

            // Verify domain event
            cart.DomainEvents.Should().ContainSingle();

            // Verify domain event
            Assert.Single(cart.DomainEvents);
            var updateEvent = Assert.IsType<CartItemQuantityUpdatedEvent>(cart.DomainEvents.First());
            Assert.Equal(2, updateEvent.OldQuantity);
            Assert.Equal(5, updateEvent.NewQuantity);
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
            Assert.Equal(36.50m, total.Amount); // (10.00 * 2) + (5.50 * 3)
        }
    }
}