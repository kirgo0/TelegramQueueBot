using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramQueueBot.DataAccess.Abstraction;

namespace TelegramQueueBot.DataAccess.Context
{
    public class MongoContext : IMongoContext
    {
        private IMongoDatabase Database { get; set; }
        public MongoClient MongoClient { get; set; }
        private readonly IConfiguration _configuration;

        public MongoContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private void ConfigureMongo()
        {
            if (MongoClient != null)
            {
                return;
            }

            // Initialize the MongoClient
            MongoClient = new MongoClient(_configuration["MongoSettings:Connection"]);

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
