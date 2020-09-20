using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Flexerant.MongoMigration
{
    class MigrationRunner : IMigrationRunner
    {
        private readonly MigrationOptions _migrationOptions;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MigrationRunner> _logger;
        private readonly IMongoDatabase _database;

        public MigrationRunner(IServiceProvider serviceProvider, IOptions<MigrationOptions> options, IMongoDatabase mongoDatabase, ILogger<MigrationRunner> logger)
        {
            _migrationOptions = options.Value;
            _serviceProvider = serviceProvider;
            _logger = logger;

            if (_migrationOptions.MongoDatabase == null)
            {
                _database = mongoDatabase;
            }
            else
            {
                _database = _migrationOptions.MongoDatabase;
            }

            if (_database == null)
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
            if (_migrationOptions.ThrowOnException)
            {
                throw new MigrationException(message, ex);
            }
            else if (_logger != null)
            {
                _logger.LogError(message);
            }
        }

        public void Run()
        {
            var collection = _database.GetCollection<MigratedItem>("Migrations");
            var filter = Builders<MigratedItem>.Filter.Empty;
            var latestMigration = collection.Find(filter).SortByDescending(x => x.MigrationNumber).Limit(1).Project(x => x.MigrationNumber).FirstOrDefault();
            Dictionary<int, Type> migrations = new Dictionary<int, Type>();

            foreach (var ass in _migrationOptions.Assemblies)
            {
                foreach (var t in ass.GetTypes())
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

            foreach (var mig in migrations.OrderBy(x => x.Key))
            {
                int migrationNumber = mig.Key;
                Type type = mig.Value;

                if (migrationNumber > latestMigration)
                {
                    try
                    {
                        var constructors = type.GetConstructors();
                        List<object> constructorParameters = new List<object>();
                        List<string> constructorErrors = new List<string>();

                        //********************************************************************************************
                        //* Cycle through each constructor and try to find any dependencies. If found, resolve them. *
                        //********************************************************************************************
                        foreach (var constructor in constructors.Where(c => c.GetParameters().Count() > 0))
                        {
                            foreach (var constructorParameter in constructor.GetParameters())
                            {
                                try
                                {
                                    Type instanceType = constructorParameter.ParameterType;
                                    var instance = _serviceProvider.GetService(instanceType);

                                    if (instance == null)
                                    {
                                        constructorErrors.Add($"Unable to resolve service for type {instanceType.FullName} while attempting to activate {type.FullName}.");
                                    }
                                    else
                                    {
                                        constructorParameters.Add(instance);
                                    }
                                }
                                catch
                                {
                                    constructorParameters.Clear();
                                }

                                if (!constructorErrors.Any())
                                {
                                    goto LoopEnd; // exit immediately.
                                }
                            }

                            if (constructorParameters.Any()) break;
                        }

                    LoopEnd:

                        Migration m;

                        if (constructorParameters.Any())
                        {
                            m = Activator.CreateInstance(type, constructorParameters.ToArray()) as Migration;
                        }
                        else
                        {
                            m = Activator.CreateInstance(type) as Migration;
                        }

                        if (_migrationOptions.SupportsTransactions)
                        {
                            //***********************************
                            //* Use transasctions if supported. *
                            //***********************************
                            using (var session = _database.Client.StartSession())
                            {
                                session.StartTransaction();

                                try
                                {
                                    m.Up(_database);
                                }
                                catch
                                {
                                    session.AbortTransaction();
                                    throw;
                                }
                            }
                        }
                        else
                        {
                            m.Up(_database);
                        }

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
