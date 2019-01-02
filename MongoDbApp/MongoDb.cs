using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoDbApp
{
    public class MongoDb
    {
        private string _connectionString;
        public string ConnectionString => _connectionString;

        public MongoDb()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString;
        }

        public MongoDb(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IMongoDatabase GetDataBase(string dbName)
        {
            var client = Initialize();

            return client.GetDatabase(dbName);
        }

        public MongoClient Initialize()
        {
            return new MongoClient(_connectionString);
        }
    }
}
