using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using LiteDB;
using LiteDB.Async;

namespace ECommerceNetApp.Persistence.Implementation.Cart
{
    public class CartDbContext : IDisposable
    {
        private const string Amount = "Amount";
        private const string Currency = "Currency";
        private const string Url = "Url";
        private const string Items = "Items";
        private const string CreatedAt = "CreatedAt";
        private const string UpdatedAt = "UpdatedAt";
        private const string AltText = "AltText";
        private const string Id = "_id";

        private readonly LiteDatabaseAsync _database;
        private bool _disposedValue;

        public CartDbContext(string connectionString)
        {
            // Register custom mappings
            BsonMapper mapper = CreateMapper();
            _database = new LiteDatabaseAsync(connectionString, mapper);
        }

        internal CartDbContext(LiteDatabaseAsync liteDatabase)
        {
            _database = liteDatabase;
        }

        public ILiteCollectionAsync<T> GetCollection<T>()
            where T : class
        {
            return _database.GetCollection<T>(typeof(T).Name);
        }

        public async Task<bool> CollectionExistsAsync<T>()
            where T : class
        {
            return await _database.CollectionExistsAsync(typeof(T).Name).ConfigureAwait(false);
        }

        public void CreateCollection<T>()
            where T : class
        {
            _database.GetCollection(typeof(T).Name);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        internal static BsonMapper CreateMapper()
        {
            var mapper = new BsonMapper();

            mapper.RegisterType(
                serialize: (money) => new BsonDocument
                {
                    [Amount] = money.Amount,
                    [Currency] = money.Currency,
                },
                deserialize: (bson) => Money.Create(bson[Amount].AsDecimal, bson[Currency].AsString));

            mapper.RegisterType(
                serialize: (imageInfo) => imageInfo == null ? null : new BsonDocument
                {
                    [Url] = imageInfo.Url,
                    [AltText] = imageInfo.AltText,
                },
                deserialize: (bson) => bson == null ? null : ImageInfo.Create(bson[Url].AsString, bson[AltText].AsString));

            // Register custom Cart serialization
            mapper.RegisterType(
                serialize: (cart) =>
                {
                    var doc = new BsonDocument
                    {
                        [Id] = cart.Id,
                        [CreatedAt] = cart.CreatedAt,
                        [UpdatedAt] = cart.UpdatedAt,
                    };

                    // Add items as an array
                    var itemsArray = new BsonArray();
                    foreach (var item in cart.Items)
                    {
                        var itemDoc = mapper.Serialize(item);
                        itemsArray.Add(itemDoc);
                    }

                    doc[Items] = itemsArray;

                    return doc;
                },
                deserialize: (bson) =>
                {
                    // Use Cart's constructor with id
                    var cart = CartEntity.Create(bson[Id].AsString);

                    // Set the readonly properties using reflection if needed
                    typeof(CartEntity).GetProperty(nameof(CartEntity.CreatedAt))?.SetValue(cart, bson[CreatedAt].AsDateTime);
                    typeof(CartEntity).GetProperty(nameof(CartEntity.UpdatedAt))?.SetValue(cart, bson[UpdatedAt].AsDateTime);

                    // Add the items
                    if (bson[Items] != null && bson[Items].IsArray)
                    {
                        foreach (var itemBson in bson[Items].AsArray)
                        {
                            var item = mapper.Deserialize<CartItem>(itemBson.AsDocument);
                            cart.AddItem(item.Id, item.Name, item.Price, item.Quantity, item.Image);
                        }
                    }

                    return cart;
                });
            return mapper;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _database?.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}
