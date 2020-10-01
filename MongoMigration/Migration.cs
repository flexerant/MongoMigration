using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Flexerant.MongoMigration
{
    public abstract class Migration
    {
        public abstract string Description { get; }
        internal MigrationAttribute MigrationAttribute => this.GetType().GetCustomAttribute<MigrationAttribute>();
        public abstract void MigrateAsTransaction(IMongoDatabase database, IClientSessionHandle session);
        public abstract void Migrate(IMongoDatabase database);
    }
}
