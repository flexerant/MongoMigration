using Flexerant.MongoMigration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;

namespace TestMigrations
{
    [Migration(3)]
    public class Migration_003 : Migration
    {
        public override string Description => "Dependency injected";

        public Migration_003(ITestDependency testDependency)
        {
            if (testDependency == null) throw new NullReferenceException();
        }

        public override void Migrate(IMongoDatabase db) { }

        public override void MigrateTransaction(IMongoDatabase database, IClientSessionHandle session) { }
    }
}
