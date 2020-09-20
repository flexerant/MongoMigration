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
using System.Linq;

namespace Tests
{
    public class MigrationRunnerTests
    {
        [Fact]
        public void SuccessFulMigration()
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
                           services.AddSingleton(database);
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
                    var studentFilter = Builders<BsonDocument>.Filter.Empty;
                    var studentCount = database.GetCollection<BsonDocument>("Students").Find(studentFilter).CountDocuments();

                    Assert.Equal(1, studentCount);

                    var migrationFilter = Builders<MigratedItem>.Filter.Empty;
                    var migrationItem = database.GetCollection<MigratedItem>("Migrations").Find(migrationFilter).ToList().Where(x => x.MigrationNumber == 1).FirstOrDefault();

                    Assert.Equal(1, migrationItem.MigrationNumber);
                    Assert.Equal("Create the students collection.", migrationItem.Description);
                }
            }
        }

        [Fact]
        public void SuccessFulMigration_from_another_assembly()
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
                           services.AddTransient<TestMigrations.ITestDependency, TestDependency>();
                           services.AddSingleton(database);
                           services.AddMongoMigrations(options =>
                           {
                               options.Assemblies.Add(typeof(TestMigrations.Migration_002).Assembly);
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
                    var classFilter = Builders<BsonDocument>.Filter.Empty;
                    var classCount = database.GetCollection<BsonDocument>("Classes").Find(classFilter).CountDocuments();

                    Assert.Equal(1, classCount);

                    var migrationFilter = Builders<MigratedItem>.Filter.Empty;
                    var migrationItems = database.GetCollection<MigratedItem>("Migrations").Find(migrationFilter).ToList();

                    Assert.Equal(3, migrationItems.Count);
                    Assert.True(migrationItems.Where(x => x.MigrationNumber == 1).Count() == 1);
                    Assert.True(migrationItems.Where(x => x.MigrationNumber == 2).Count() == 1);
                    Assert.True(migrationItems.Where(x => x.MigrationNumber == 3).Count() == 1);
                }
            }
        }

        [Fact]
        public void InvalidOperationException_when_IMongoDatabase_dependency_is_missing()
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

                Assert.Throws<InvalidOperationException>(() =>
                {
                    using (var host = builder.Start()) { }
                });
            }
        }
    }
}
