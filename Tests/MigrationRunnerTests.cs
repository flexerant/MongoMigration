using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System;
using Xunit;
using Flexerant.MongoMigration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using System.Linq;
using Moq;

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
                           services.AddMongoMigrations();
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
                       services.AddMongoMigrations();
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

        [Fact]
        public void AlternateIMongoDatabase()
        {
            using (var runner = new TestDatabaseRunner())
            {
                var iDatabaseMock = new Mock<IMongoDatabase>();
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
                               options.MongoDatabase = iDatabaseMock.Object;
                           });
                       })
                       .Configure(app => { });
                    });

                using (var host = builder.Start())
                {
                    var migrationRunner = host.Services.GetService<IMigrationRunner>() as MigrationRunner;

                    Assert.Equal(iDatabaseMock.Object.GetType(), migrationRunner.Database.GetType());
                    Assert.NotEqual(database.GetType(), migrationRunner.Database.GetType());
                }
            }
        }
    }
}
