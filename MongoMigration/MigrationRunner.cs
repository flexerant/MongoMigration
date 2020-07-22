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

        public MigrationRunner(IServiceProvider serviceProvider, IOptions<MigrationOptions> options)
        {
            _migrationOptions = options.Value;
            _serviceProvider = serviceProvider;
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
            else if (_migrationOptions.Logger != null)
            {
                _migrationOptions.Logger.LogError(message);
            }
        }

        public void Run()
        {
            //var serviceProvider = _serviceCollection.BuildServiceProvider();
            var database = _migrationOptions.MongoDatabase;
            var collection = database.GetCollection<MigratedItem>("Migrations");
            var filter = Builders<MigratedItem>.Filter.Empty;
            var latestMigration = collection.Find(filter).SortByDescending(x => x.MigrationNumber).Limit(1).Project(x => x.MigrationNumber).FirstOrDefault();
            Dictionary<int, Type> migrations = new Dictionary<int, Type>();


            foreach (var ass in _migrationOptions.Assemblies)
            {
                foreach (var t in ass.GetTypes())
                {
                    MigrationAttribute att = t.GetCustomAttribute<MigrationAttribute>();

                    if (att != null)
                    {
                        if (typeof(Migration).IsAssignableFrom(t))
                        {
                            if (migrations.ContainsKey(att.MigrationNumber))
                            {
                                this.HandleException($"Migration {att.MigrationNumber} on {t.FullName} has already been registered on {migrations[att.MigrationNumber].FullName}.");
                            }
                            else
                            {
                                migrations.Add(att.MigrationNumber, t);
                            }
                        }
                        else
                        {
                            this.HandleException($"The type '{t.FullName}' does not inherit from '{typeof(Migration).FullName}'. Migration {att.MigrationNumber} could not be run.");
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
                    using (var session = database.Client.StartSession())
                    {
                        session.StartTransaction();

                        try
                        {
                            var constructors = type.GetConstructors();

                            //var hasParameterizedConstructors = constructors.Any(c => c.GetParameters().Count() > 0);
                            var constructor = constructors.Where(c => c.GetParameters().Count() > 0).FirstOrDefault();
                            List<object> constructorParameters = new List<object>();

                            if (constructor == null)
                            {
                                constructor = constructors.FirstOrDefault();
                            }

                            foreach (var constructorParameter in constructor.GetParameters())
                            {
                                Type instanceType = constructorParameter.ParameterType;

                                var instance = _serviceProvider.GetService(instanceType);

                                if (instance == null) throw new InvalidOperationException($"Unable to resolve service for type {instanceType.FullName} while attempting to activate {type.FullName}.");

                                constructorParameters.Add(instance);
                            }

                            Migration m = Activator.CreateInstance(type, constructorParameters.ToArray()) as Migration;

                            m.Up(database);
                            //m.Down(database);

                            collection.InsertOne(new MigratedItem()
                            {
                                MigrationNumber = migrationNumber,
                                Description = m.Description,
                                Type = type.FullName,
                                Assembly = type.Assembly.GetName().Name,
                                TimeStamp = DateTime.UtcNow
                            });
                        }
                        catch (Exception ex)
                        {
                            session.AbortTransaction();

                            this.HandleException($"An error occurred migrating '{type.FullName}' to version {migrationNumber}.", ex);
                        }
                    }

                }
            }
        }
    }
}
