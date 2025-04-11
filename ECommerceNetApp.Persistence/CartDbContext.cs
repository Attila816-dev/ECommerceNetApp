using LiteDB.Async;

namespace ECommerceNetApp.Persistence
{
    public class CartDbContext : IDisposable
    {
        private readonly LiteDatabaseAsync _database;
        private bool _disposedValue;

        public CartDbContext(string connectionString)
        {
            _database = new LiteDatabaseAsync(connectionString);
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
