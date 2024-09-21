using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TelegramQueueBot.Data.Abstraction;

namespace Data.Context
{
    public class MongoContext : IMongoContext
    {
        private IMongoDatabase Database { get; set; }
        public MongoClient MongoClient { get; set; }
        private readonly IConfiguration _configuration;
        private readonly ILogger<MongoContext> _log;

        public MongoContext(IConfiguration configuration, ILogger<MongoContext> log)
        {
            _configuration = configuration;
            _log = log;
            ConfigureMongo();
        }

        private void ConfigureMongo()
        {
            if (MongoClient != null)
            {
                return;
            }
            var connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION");

            var settings = MongoClientSettings.FromConnectionString(connectionString);

            settings.SocketTimeout = TimeSpan.FromSeconds(10);
            settings.ConnectTimeout = TimeSpan.FromSeconds(10);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(3);

            try
            {
                MongoClient = new MongoClient(settings);
            } catch(Exception ex)
            {
                _log.LogCritical(ex, "Can't connect to the database {connection}", connectionString);
                throw;
            }

            // Check if the database exists
            var dbName = Environment.GetEnvironmentVariable("MONGO_DATABASE");

            Database = MongoClient.GetDatabase(dbName);
            if (Database is null)
            {
                throw new Exception("Database wasn't found or created");
            }
            Database.ListCollectionNames().ToList();
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            ConfigureMongo();

            // Check if the collection exists
            var collectionList = Database.ListCollectionNames().ToList();
            if (!collectionList.Contains(name))
            {
                // Create the collection
                Database.CreateCollection(name);
            }

            return Database.GetCollection<T>(name);
        }

    }
}
