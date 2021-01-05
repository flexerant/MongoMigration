using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flexerant.MongoMigration
{
    interface IMigrationRunner
    {
        void Run();
        IMongoDatabase Database { get; set; }
    }
}
