using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Exceptions.Cart;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Implementation.Cart;
using LiteDB.Async;
using Moq;
using Shouldly;

namespace ECommerceNetApp.Persistence.UnitTest.Repositories
{
    public class CartRepositoryTests : IDisposable
    {
        private readonly CartDbContext _dbContext;
        private readonly CartRepository _cartRepository;
        private readonly Mock<IDomainEventService> _mockDomainEventService;
        private bool disposedValue;

        public CartRepositoryTests()
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            var liteDatabase = new LiteDatabaseAsync(new MemoryStream(), CartDbContext.CreateMapper());
#pragma warning restore CA2000 // Dispose objects before losing scope
            _dbContext = new CartDbContext(liteDatabase);

            _mockDomainEventService = new Mock<IDomainEventService>();
            _cartRepository = new CartRepository(_dbContext, _mockDomainEventService.Object);
        }

        [Fact]
        public async Task GetCartAsync_ValidId_ReturnsCart()
        {
            // Arrange
            var cartId = "test-cart-123";
            var expectedCart = new CartEntity(cartId);

            var cartCollection = _dbContext.GetCollection<CartEntity>();
            await cartCollection.InsertAsync(expectedCart);

            // Act
            var result = await _cartRepository.GetByIdAsync(cartId, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result!.Id.ShouldBe(cartId);
        }

        [Fact]
        public async Task GetCartAsync_InvalidId_ReturnsNull()
        {
            // Arrange
            var cartId = "nonexistent-cart";

            var cartCollection = _dbContext.GetCollection<CartEntity>();

            // Act
            var result = await _cartRepository.GetByIdAsync(cartId, CancellationToken.None);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task SaveCartAsync_ValidCart_SavesSuccessfully()
        {
            // Arrange
            var cart = new CartEntity("cart-123");

            var cartCollection = _dbContext.GetCollection<CartEntity>();

            // Act
            await _cartRepository.SaveAsync(cart, CancellationToken.None);

            // Assert
            var insertedCart = await cartCollection.FindByIdAsync(cart.Id);
            insertedCart.ShouldNotBeNull();
        }

        [Fact]
        public async Task SaveAndGetCart_Success()
        {
            // Arrange
            var cart = new CartEntity("integration-test-cart");
            cart.AddItem(1, "Test Item", Money.From(10.99m), 2);

            // Act
            await _cartRepository.SaveAsync(cart, CancellationToken.None);
            var retrievedCart = await _cartRepository.GetByIdAsync(cart.Id, CancellationToken.None);

            // Assert
            retrievedCart.ShouldNotBeNull();
            retrievedCart.Id.ShouldBe(cart.Id);
            retrievedCart.Items.Count.ShouldBe(1);

            var item = retrievedCart.Items.First();
            item.Name.ShouldBe("Test Item");
            item.Price!.Amount.ShouldBe(10.99m);
            item.Quantity.ShouldBe(2);
        }

        [Fact]
        public async Task DeleteCart_Success()
        {
            // Arrange
            var cart = new CartEntity("cart-to-delete");
            cart.AddItem(1, "Item to delete", Money.From(15.99m), 1);

            await _cartRepository.SaveAsync(cart, CancellationToken.None);

            // Act
            await _cartRepository.DeleteAsync(cart.Id, CancellationToken.None);
            var retrievedCart = await _cartRepository.GetByIdAsync(cart.Id, CancellationToken.None);

            // Assert
            retrievedCart.ShouldBeNull();
        }

        [Fact]
        public async Task DeleteCartAsync_ValidId_DeletesSuccessfully()
        {
            // Arrange
            var cartId = "cart-to-delete";
            var expectedCart = new CartEntity(cartId);

            var cartCollection = _dbContext.GetCollection<CartEntity>();
            await cartCollection.InsertAsync(expectedCart);

            // Act
            await _cartRepository.DeleteAsync(cartId, CancellationToken.None);

            // Assert
            (await cartCollection.CountAsync()).ShouldBe(0);
        }

        [Fact]
        public async Task DeleteCartAsync_NonexistentId_ThrowsException()
        {
            // Arrange
            var cartId = "nonexistent-cart";
            var cartCollection = _dbContext.GetCollection<CartEntity>();

            // Act
            await Should.ThrowAsync<InvalidCartException>(async () =>
            {
                await _cartRepository.DeleteAsync(cartId, CancellationToken.None);
            });

            // Assert
            (await cartCollection.CountAsync()).ShouldBe(0);
        }

        [Fact]
        public async Task AddItem_WithHighQuantity_SuccessfullyAddsItem()
        {
            // Arrange
            var cart = new CartEntity("cart-high-values");
            var quantity = int.MaxValue;
            var price = Money.From(10);

            // Act
            cart.AddItem(1, "High Quantity Item", price, quantity);
            await _cartRepository.SaveAsync(cart, CancellationToken.None);
            var retrievedCart = await _cartRepository.GetByIdAsync(cart.Id, CancellationToken.None);

            // Assert
            retrievedCart.ShouldNotBeNull();
            retrievedCart.Items.Count.ShouldBe(1);

            var item = retrievedCart.Items.First();
            item.Name.ShouldBe("High Quantity Item");
            item.Price!.Amount.ShouldBe(price.Amount);
            item.Quantity.ShouldBe(quantity);
        }

        [Fact]
        public async Task AddItem_WithHighPrice_SuccessfullyAddsItem()
        {
            // Arrange
            var cart = new CartEntity("cart-high-values");
            var quantity = 2;
            var price = Money.From(int.MaxValue);

            // Act
            cart.AddItem(1, "High Quantity Item", price, quantity);
            await _cartRepository.SaveAsync(cart, CancellationToken.None);
            var retrievedCart = await _cartRepository.GetByIdAsync(cart.Id, CancellationToken.None);

            // Assert
            retrievedCart.ShouldNotBeNull();
            retrievedCart.Items.Count.ShouldBe(1);

            var item = retrievedCart.Items.First();
            item.Name.ShouldBe("High Quantity Item");
            item.Price!.Amount.ShouldBe(price.Amount);
            item.Quantity.ShouldBe(quantity);
        }

        [Fact]
        public async Task AddItem_SpecialCharactersInName_Success()
        {
            // Arrange
            var cart = new CartEntity("cart-special-characters");
            var specialName = "Item@#%&*()!";

            // Act
            cart.AddItem(1, specialName, Money.From(19.99m), 1);
            await _cartRepository.SaveAsync(cart, CancellationToken.None);
            var retrievedCart = await _cartRepository.GetByIdAsync(cart.Id, CancellationToken.None);

            // Assert
            retrievedCart.ShouldNotBeNull();
            retrievedCart.Items.Count.ShouldBe(1);

            var item = retrievedCart.Items.First();
            item.Name.ShouldBe(specialName);
            item.Price!.Amount.ShouldBe(19.99m);
            item.Quantity.ShouldBe(1);
        }

        [Fact]
        public async Task AddItem_ToNonexistentCart_AddedSuccessfully()
        {
            // Arrange
            var nonexistentCartId = "nonexistent-cart";
            var cart = new CartEntity(nonexistentCartId);

            // Act
            cart.AddItem(1, "Test Item", Money.From(10.99m), 1);
            await _cartRepository.SaveAsync(cart, CancellationToken.None);

            // Assert
            var retrievedCart = await _cartRepository.GetByIdAsync(cart.Id, CancellationToken.None);
            retrievedCart.ShouldNotBeNull();
            retrievedCart.Items.Count.ShouldBe(1);

            var item = retrievedCart.Items.First();
            item.Name.ShouldBe("Test Item");
            item.Price!.Amount.ShouldBe(10.99m);
            item.Quantity.ShouldBe(1);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _dbContext.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }
    }
}