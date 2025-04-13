using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using LiteDB;
using LiteDB.Async;

namespace ECommerceNetApp.Persistence.Implementation
{
    public class CartDbContext : IDisposable
    {
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
                    ["Amount"] = money.Amount,
                    ["Currency"] = money.Currency,
                },
                deserialize: (bson) => new Money(
                    bson["Amount"].AsDecimal,
                    bson["Currency"].AsString));

            mapper.RegisterType(
                serialize: (imageInfo) => imageInfo == null ? null : new BsonDocument
                {
                    ["Url"] = imageInfo.Url,
                    ["AltText"] = imageInfo.AltText,
                },
                deserialize: (bson) => bson == null
                    ? null
                    : new ImageInfo(bson["Url"].AsString, bson["AltText"].AsString));

            // Register custom Cart serialization
            mapper.RegisterType(
                serialize: (cart) =>
                {
                    var doc = new BsonDocument
                    {
                        ["_id"] = cart.Id,
                        ["CreatedAt"] = cart.CreatedAt,
                        ["UpdatedAt"] = cart.UpdatedAt,
                    };

                    // Add items as an array
                    var itemsArray = new BsonArray();
                    foreach (var item in cart.Items)
                    {
                        var itemDoc = mapper.Serialize(item);
                        itemsArray.Add(itemDoc);
                    }

                    doc["Items"] = itemsArray;

                    return doc;
                },
                deserialize: (bson) =>
                {
                    // Use Cart's constructor with id
                    var cart = new Cart(bson["_id"].AsString);

                    // Set the readonly properties using reflection if needed
                    typeof(Cart).GetProperty(nameof(Cart.CreatedAt))?.SetValue(cart, bson["CreatedAt"].AsDateTime);
                    typeof(Cart).GetProperty(nameof(Cart.UpdatedAt))?.SetValue(cart, bson["UpdatedAt"].AsDateTime);

                    // Add the items
                    if (bson["Items"] != null && bson["Items"].IsArray)
                    {
                        foreach (var itemBson in bson["Items"].AsArray)
                        {
                            var item = mapper.Deserialize<CartItem>(itemBson.AsDocument);

                            // Use reflection or a method to add the item
                            cart.AddItem(item);
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
