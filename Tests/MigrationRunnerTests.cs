using Microsoft.Extensions.Hosting;
using Mongo2Go;
using MongoDB.Driver;
using System;
using Xunit;
using Flexerant;
using Flexerant.MongoMigration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using MongoDB.Bson;

namespace Tests
{
    public class MigrationRunnerTests : IDisposable
    {
        internal static MongoDbRunner _runner;
        static IMongoDatabase _database;

        static MigrationRunnerTests()
        {
            _runner = MongoDbRunner.Start();

            MongoClient client = new MongoClient(_runner.ConnectionString);

            _database = client.GetDatabase(typeof(MigrationRunnerTests).Name);
        }

        public void Dispose()
        {
            ((IDisposable)_runner).Dispose();
        }

        private IMongoCollection<T> GetCollection<T>()
        {
            return _database.GetCollection<T>($"{typeof(T).Name}s");
        }

        [Fact]
        public void Test1()
        {
            var builder = new HostBuilder()
                .ConfigureWebHost(config =>
                {
                    config.UseTestServer()
                   .ConfigureServices(services =>
                   {
                       services.AddSingleton<IMongoDatabase>(_database);
                       services.AddMongoMigrations(options =>
                       {
                           options.SupportsTransactions = false;
                       });
                   })
                   .Configure(app =>
                   {
                       app.UseMongoMigrations();
                   });
                })
                .Start();

            var filter = Builders<BsonDocument>.Filter.Empty;
            var count = _database.GetCollection<BsonDocument>("Students").Find(filter).CountDocuments();

            Assert.Equal(1, count);
        }
    }
}
