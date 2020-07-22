using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Flexerant.MongoMigration
{
    public class MigrationOptions
    {
        public List<Assembly> Assemblies = new List<Assembly>();
        public ILogger Logger { get; set; }
        public IMongoDatabase MongoDatabase { get; set; }
        public bool ThrowOnException { get; set; } = true;
    }
}
