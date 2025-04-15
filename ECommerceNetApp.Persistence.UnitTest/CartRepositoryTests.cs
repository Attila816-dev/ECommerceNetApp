using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Implementation.Cart;
using LiteDB.Async;
using Shouldly;

namespace ECommerceNetApp.Persistence.UnitTest
{
    public class CartRepositoryTests : IDisposable
    {
        private readonly CartDbContext _dbContext;
        private readonly CartRepository _repository;
        private bool disposedValue;

        public CartRepositoryTests()
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            var liteDatabase = new LiteDatabaseAsync(new MemoryStream(), CartDbContext.CreateMapper());
#pragma warning restore CA2000 // Dispose objects before losing scope

            _dbContext = new CartDbContext(liteDatabase);
            _repository = new CartRepository(_dbContext);
        }

        [Fact]
        public async Task GetCartAsync_ValidId_ReturnsCart()
        {
            // Arrange
            var cartId = "test-cart-123";
            var expectedCart = new Cart(cartId);

            var cartCollection = _dbContext.GetCollection<Cart>();
            await cartCollection.InsertAsync(expectedCart);

            // Act
            var result = await _repository.GetByIdAsync(cartId, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result!.Id.ShouldBe(cartId);
        }

        [Fact]
        public async Task GetCartAsync_InvalidId_ReturnsNull()
        {
            // Arrange
            var cartId = "nonexistent-cart";

            var cartCollection = _dbContext.GetCollection<Cart>();

            // Act
            var result = await _repository.GetByIdAsync(cartId, CancellationToken.None);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task SaveCartAsync_ValidCart_SavesSuccessfully()
        {
            // Arrange
            var cart = new Cart("cart-123");

            var cartCollection = _dbContext.GetCollection<Cart>();

            // Act
            await _repository.SaveAsync(cart, CancellationToken.None);

            // Assert
            var insertedCart = await cartCollection.FindByIdAsync(cart.Id);
            insertedCart.ShouldNotBeNull();
        }

        [Fact]
        public async Task SaveAndGetCart_Success()
        {
            // Arrange
            var cart = new Cart("integration-test-cart");
            cart.AddItem(new CartItem(1, "Test Item", Money.From(10.99m), 2));

            // Act
            await _repository.SaveAsync(cart, CancellationToken.None);
            var retrievedCart = await _repository.GetByIdAsync(cart.Id, CancellationToken.None);

            // Assert
            Assert.NotNull(retrievedCart);
            Assert.Equal(cart.Id, retrievedCart.Id);
            Assert.Single(retrievedCart.Items);

            var item = retrievedCart.Items.First();
            Assert.Equal("Test Item", item.Name);
            Assert.Equal(10.99m, item.Price!.Amount);
            Assert.Equal(2, item.Quantity);
        }

        [Fact]
        public async Task DeleteCart_Success()
        {
            // Arrange
            var cart = new Cart("cart-to-delete");
            cart.AddItem(new CartItem(1, "Item to delete", Money.From(15.99m), 1));

            await _repository.SaveAsync(cart, CancellationToken.None);

            // Act
            await _repository.DeleteAsync(cart.Id, CancellationToken.None);
            var retrievedCart = await _repository.GetByIdAsync(cart.Id, CancellationToken.None);

            // Assert
            Assert.Null(retrievedCart);
        }

        [Fact]
        public async Task DeleteCartAsync_ValidId_DeletesSuccessfully()
        {
            // Arrange
            var cartId = "cart-to-delete";
            var expectedCart = new Cart(cartId);

            var cartCollection = _dbContext.GetCollection<Cart>();
            await cartCollection.InsertAsync(expectedCart);

            // Act
            await _repository.DeleteAsync(cartId, CancellationToken.None);

            // Assert
            (await cartCollection.CountAsync()).ShouldBe(0);
        }

        [Fact]
        public async Task DeleteCartAsync_NonexistentId_CallsDeleteAnyway()
        {
            // Arrange
            var cartId = "nonexistent-cart";
            var cartCollection = _dbContext.GetCollection<Cart>();

            // Act
            await _repository.DeleteAsync(cartId, CancellationToken.None);

            // Assert
            (await cartCollection.CountAsync()).ShouldBe(0);
        }

        [Fact]
        public void Constructor_NullDbContext_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new CartRepository(null));
            exception.ParamName.ShouldBe("dbContext");
        }

        [Fact]
        public async Task AddItem_WithHighQuantity_SuccessfullyAddsItem()
        {
            // Arrange
            var cart = new Cart("cart-high-values");
            var quantity = int.MaxValue;
            var price = Money.From(10);

            // Act
            cart.AddItem(new CartItem(1, "High Quantity Item", price, quantity));
            await _repository.SaveAsync(cart, CancellationToken.None);
            var retrievedCart = await _repository.GetByIdAsync(cart.Id, CancellationToken.None);

            // Assert
            Assert.NotNull(retrievedCart);
            Assert.Single(retrievedCart.Items);

            var item = retrievedCart.Items.First();
            Assert.Equal("High Quantity Item", item.Name);
            Assert.Equal(price.Amount, item.Price!.Amount);
            Assert.Equal(quantity, item.Quantity);
        }

        [Fact]
        public async Task AddItem_WithHighPrice_SuccessfullyAddsItem()
        {
            // Arrange
            var cart = new Cart("cart-high-values");
            var quantity = 2;
            var price = Money.From(int.MaxValue);

            // Act
            cart.AddItem(new CartItem(1, "High Quantity Item", price, quantity));
            await _repository.SaveAsync(cart, CancellationToken.None);
            var retrievedCart = await _repository.GetByIdAsync(cart.Id, CancellationToken.None);

            // Assert
            Assert.NotNull(retrievedCart);
            Assert.Single(retrievedCart.Items);

            var item = retrievedCart.Items.First();
            Assert.Equal("High Quantity Item", item.Name);
            Assert.Equal(price.Amount, item.Price!.Amount);
            Assert.Equal(quantity, item.Quantity);
        }

        [Fact]
        public async Task AddItem_SpecialCharactersInName_Success()
        {
            // Arrange
            var cart = new Cart("cart-special-characters");
            var specialName = "Item@#%&*()!";

            // Act
            cart.AddItem(new CartItem(1, specialName, Money.From(19.99m), 1));
            await _repository.SaveAsync(cart, CancellationToken.None);
            var retrievedCart = await _repository.GetByIdAsync(cart.Id, CancellationToken.None);

            // Assert
            Assert.NotNull(retrievedCart);
            Assert.Single(retrievedCart.Items);

            var item = retrievedCart.Items.First();
            Assert.Equal(specialName, item.Name);
            Assert.Equal(19.99m, item.Price!.Amount);
            Assert.Equal(1, item.Quantity);
        }

        [Fact]
        public async Task AddItem_ToNonexistentCart_AddedSuccessfully()
        {
            // Arrange
            var nonexistentCartId = "nonexistent-cart";
            var cart = new Cart(nonexistentCartId);

            // Act
            cart.AddItem(new CartItem(1, "Test Item", Money.From(10.99m), 1));
            await _repository.SaveAsync(cart, CancellationToken.None);

            // Assert
            var retrievedCart = await _repository.GetByIdAsync(cart.Id, CancellationToken.None);
            Assert.NotNull(retrievedCart);
            Assert.Single(retrievedCart.Items);

            var item = retrievedCart.Items.First();
            Assert.Equal("Test Item", item.Name);
            Assert.Equal(10.99m, item.Price!.Amount);
            Assert.Equal(1, item.Quantity);
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