using LiteDB.Async;

namespace ECommerceNetApp.Persistence
{
    public class CartDbContext
    {
        private readonly LiteDatabaseAsync _database;

        public CartDbContext(string connectionString)
        {
            _database = new LiteDatabaseAsync(connectionString);
        }

        public ILiteCollectionAsync<T> GetCollection<T>() where T : class
        {
            return _database.GetCollection<T>(typeof(T).Name);
        }

        public async Task<bool> CollectionExistsAsync<T>() where T : class
        {
            return await _database.CollectionExistsAsync(typeof(T).Name);
        }

        public void CreateCollection<T>() where T : class
        {
            _database.GetCollection(typeof(T).Name);
        }
    }
}
