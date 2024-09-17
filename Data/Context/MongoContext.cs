using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TelegramQueueBot.Data.Abstraction;

namespace TelegramQueueBot.Data.Context
{
    public class MongoContext : IMongoContext
    {
        private IMongoDatabase Database { get; set; }
        public MongoClient MongoClient { get; set; }
        private readonly IConfiguration _configuration;
        private ILogger<MongoContext> _log;

        public MongoContext(IConfiguration configuration, ILogger<MongoContext> log)
        {
            _configuration = configuration;
            _log = log;
        }

        private void ConfigureMongo()
        {
            if (MongoClient != null)
            {
                return;
            }

            // Initialize the MongoClient
            try
            {
                MongoClient = new MongoClient(_configuration["MongoSettings:Connection"]);
            } catch(Exception)
            {
                _log.LogCritical("Can't connect to the database {connection}", _configuration["MongoSettings:Connection"]);
                throw;
            }

            // Check if the database exists
            var dbName = _configuration["MongoSettings:DatabaseName"];
            var databaseList = MongoClient.ListDatabaseNames().ToList();
            if (!databaseList.Contains(dbName))
            {
                // Create the database by accessing it
                Database = MongoClient.GetDatabase(dbName);
            }
            else
            {
                Database = MongoClient.GetDatabase(dbName);
            }
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
