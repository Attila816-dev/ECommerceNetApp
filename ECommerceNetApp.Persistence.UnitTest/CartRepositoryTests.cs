using ECommerceNetApp.Domain;
using FluentAssertions;
using LiteDB.Async;

namespace ECommerceNetApp.Persistence.UnitTest
{
    public class CartRepositoryTests : IDisposable
    {
        private readonly CartDbContext _dbContext;
        private readonly CartRepository _repository;
        private readonly LiteDatabaseAsync _liteDatabase;
        private bool disposedValue;

        public CartRepositoryTests()
        {
            _liteDatabase = new LiteDatabaseAsync(new MemoryStream());
            _dbContext = new CartDbContext(_liteDatabase);
            _repository = new CartRepository(_dbContext);
        }

        [Fact]
        public async Task GetCartAsync_ValidId_ReturnsCart()
        {
            // Arrange
            var cartId = "test-cart-123";
            var expectedCart = new Cart { Id = cartId };

            var cartCollection = _dbContext.GetCollection<Cart>();
            await cartCollection.InsertAsync(expectedCart);

            // Act
            var result = await _repository.GetCartAsync(cartId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(cartId);
        }

        [Fact]
        public async Task GetCartAsync_InvalidId_ReturnsNull()
        {
            // Arrange
            var cartId = "nonexistent-cart";

            var cartCollection = _dbContext.GetCollection<Cart>();

            // Act
            var result = await _repository.GetCartAsync(cartId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task SaveCartAsync_ValidCart_SavesSuccessfully()
        {
            // Arrange
            var cart = new Cart { Id = "cart-123" };
            var initialUpdateTime = cart.UpdatedAt;

            var cartCollection = _dbContext.GetCollection<Cart>();
            await cartCollection.InsertAsync(cart);

            // Act
            await _repository.SaveCartAsync(cart);

            // Assert
            cart.UpdatedAt.Should().BeAfter(initialUpdateTime);
        }

        [Fact]
        public async Task SaveCartAsync_EmptyCartId_ThrowsArgumentException()
        {
            // Arrange
            var cart = new Cart { Id = string.Empty };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _repository.SaveCartAsync(cart));
            exception.Message.Should().Contain("Cart Id cannot be empty");
        }

        [Fact]
        public async Task SaveCartAsync_NullCartId_ThrowsArgumentException()
        {
            // Arrange
            var cart = new Cart { Id = null };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _repository.SaveCartAsync(cart));
            exception.Message.Should().Contain("Cart Id cannot be empty");
        }

        [Fact]
        public async Task DeleteCartAsync_ValidId_DeletesSuccessfully()
        {
            // Arrange
            var cartId = "cart-to-delete";
            var expectedCart = new Cart { Id = cartId };

            var cartCollection = _dbContext.GetCollection<Cart>();
            await cartCollection.InsertAsync(expectedCart);

            // Act
            await _repository.DeleteCartAsync(cartId);

            // Assert
            (await cartCollection.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task DeleteCartAsync_NonexistentId_CallsDeleteAnyway()
        {
            // Arrange
            var cartId = "nonexistent-cart";
            var cartCollection = _dbContext.GetCollection<Cart>();

            // Act
            await _repository.DeleteCartAsync(cartId);

            // Assert
            (await cartCollection.CountAsync()).Should().Be(0);
        }

        [Fact]
        public void Constructor_NullDbContext_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new CartRepository(null));
            exception.ParamName.Should().Be("CartDbContext");
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
                    _liteDatabase.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }
    }
}