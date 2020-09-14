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
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;

namespace Tests
{
    public class MigrationRunnerTests
    {
        [Fact]
        public void Test1()
        {
            using (var runner = new TestDatabaseRunner())
            {
                var database = runner.Database;
                var builder = new HostBuilder()
                    .ConfigureWebHost(config =>
                    {
                        config.UseTestServer()
                       .ConfigureServices(services =>
                       {
                           services.AddSingleton<IMongoDatabase>(database);
                           services.AddMongoMigrations(options =>
                           {
                               options.SupportsTransactions = false;
                           });
                       })
                       .Configure(app =>
                       {
                           app.UseMongoMigrations();
                       });
                    });

                using (var host = builder.Start())
                {
                    var filter = Builders<BsonDocument>.Filter.Empty;
                    var count = database.GetCollection<BsonDocument>("Students").Find(filter).CountDocuments();

                    Assert.Equal(1, count);
                }
            }
        }

        [Fact]
        public void Test2()
        {
            using (var runner = new TestDatabaseRunner())
            {
                var database = runner.Database;
                var builder = new HostBuilder()
                .ConfigureWebHost(config =>
                {
                    config.UseTestServer()
                   .ConfigureServices(services =>
                   {
                       services.AddSingleton<IMongoDatabase>(database);
                       services.AddMongoMigrations(options =>
                       {
                           options.SupportsTransactions = false;
                       });
                   })
                   .Configure(app =>
                   {
                       app.UseMongoMigrations();
                   });
                });

                using (var host = builder.Start())
                {
                    var filter = Builders<BsonDocument>.Filter.Empty;
                    var count = database.GetCollection<BsonDocument>("Students").Find(filter).CountDocuments();

                    Assert.Equal(1, count);
                }
            }
        }
    }
}
