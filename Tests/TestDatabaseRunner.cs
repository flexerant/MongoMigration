using Mongo2Go;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    public class TestDatabaseRunner : IDisposable
    {
        private MongoDbRunner _runner;
        public IMongoDatabase Database { get; private set; }

        public TestDatabaseRunner()
        {
            _runner = MongoDbRunner.Start();

            MongoClient client = new MongoClient(_runner.ConnectionString);

            this.Database = client.GetDatabase(typeof(MigrationRunnerTests).Name);
        }
        
        public IMongoCollection<T> GetCollection<T>()
        {
            return this.Database.GetCollection<T>($"{typeof(T).Name}s");
        }


        public void Dispose()
        {
            ((IDisposable)_runner).Dispose();
        }
    }
}
