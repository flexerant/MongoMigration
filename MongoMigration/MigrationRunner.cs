using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Flexerant.MongoMigration
{
    class MigrationRunner : IMigrationRunner
    {
        private readonly MigrationOptions _migrationOptions;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MigrationRunner> _logger;

        internal IMongoDatabase Database;

        public MigrationRunner(IServiceProvider serviceProvider, IOptions<MigrationOptions> options, IMongoDatabase mongoDatabase, ILogger<MigrationRunner> logger)
        {
            _migrationOptions = options.Value;
            _serviceProvider = serviceProvider;
            _logger = logger;

            if (_migrationOptions.MongoDatabase == null)
            {
                Database = mongoDatabase;
            }
            else
            {
                Database = _migrationOptions.MongoDatabase;
            }

            if (Database == null)
            {
                throw new MigrationException($"The dependency '{typeof(IMongoDatabase).FullName}' could not be found.");
            }
        }

        private void HandleException(string message)
        {
            this.HandleException(message, null);
        }

        private void HandleException(string message, Exception ex)
        {
            if (_logger != null)
            {
                _logger.LogError(message);
            }

            throw new MigrationException(message, ex);
        }

        public void Run()
        {
            var collection = Database.GetCollection<MigratedItem>("Migrations");
            var filter = Builders<MigratedItem>.Filter.Empty;
            var latestMigration = collection.Find(filter).SortByDescending(x => x.MigrationNumber).Limit(1).Project(x => x.MigrationNumber).FirstOrDefault();
            Dictionary<int, Type> migrations = new Dictionary<int, Type>();

            foreach (var ass in _migrationOptions.Assemblies)
            {
                foreach (var t in ass.GetTypes())
                {
                    if (t != typeof(Migration))
                    {
                        if (typeof(Migration).IsAssignableFrom(t))
                        {
                            MigrationAttribute att = t.GetCustomAttribute<MigrationAttribute>();

                            if (att == null) this.HandleException($"The type '{t.FullName}' must be decorated with '{typeof(MigrationAttribute).FullName}'.");

                            if (migrations.ContainsKey(att.MigrationNumber))
                            {
                                this.HandleException($"Migration {att.MigrationNumber} on {t.FullName} has already been registered on {migrations[att.MigrationNumber].FullName}.");
                            }
                            else
                            {
                                migrations.Add(att.MigrationNumber, t);
                            }
                        }
                    }
                }
            }

            foreach (var mig in migrations.OrderBy(x => x.Key))
            {
                int migrationNumber = mig.Key;
                Type type = mig.Value;

                if (migrationNumber > latestMigration)
                {
                    try
                    {
                        Migration m = ActivatorUtilities.CreateInstance(_serviceProvider, type) as Migration;                        

                        try
                        {
                            //***********************************
                            //* Use transasctions if supported. *
                            //***********************************
                            using (var session = Database.Client.StartSession())
                            {
                                try
                                {                                 
                                    m.MigrateAsTransaction(this.Database, session);

                                    if (session.IsInTransaction) session.CommitTransaction();                                    
                                }
                                catch
                                {
                                    if (session.IsInTransaction) session.AbortTransaction();
                                    throw;
                                }
                            }
                        }
                        catch
                        {
                            throw;
                        }

                        m.Migrate(this.Database);

                        MigratedItem migratedItem = new MigratedItem()
                        {
                            MigrationNumber = migrationNumber,
                            Description = m.Description,
                            Type = type.FullName,
                            Assembly = type.Assembly.GetName().Name,
                            TimeStamp = DateTime.UtcNow
                        };

                        collection.InsertOne(migratedItem);

                        if (_logger != null)
                        {
                            _logger.LogInformation("Successfully migrated to {MigrationNumber}.", migrationNumber);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.HandleException($"An error occurred migrating '{type.FullName}' to version {migrationNumber}.", ex);
                    }
                }
            }
        }
    }
}
