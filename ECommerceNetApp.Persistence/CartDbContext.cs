using LiteDB;

namespace ECommerceNetApp.Persistence
{
    public class CartDbContext
    {
        private readonly LiteDatabase _database;

        public CartDbContext(string connectionString)
        {
            _database = new LiteDatabase(connectionString);
        }

        public ILiteCollection<T> GetCollection<T>() where T : class
        {
            return _database.GetCollection<T>(typeof(T).Name);
        }

        public bool CollectionExists(string name)
        {
            return _database.CollectionExists(name);
        }

        public void CreateCollection(string name)
        {
            _database.GetCollection(name);
        }
    }
}
