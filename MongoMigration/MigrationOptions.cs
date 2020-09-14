﻿using Microsoft.Extensions.Logging;
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
        //public ILogger Logger { get; set; }
        public IMongoDatabase MongoDatabase { get; set; } = null;
        public bool ThrowOnException { get; set; } = true;
        public bool SupportsTransactions { get; set; } = true;
        //public event MigrationUpdatedHandler MigratedUp;

        //public MigrationUpdatedHandler Handler { get; set; }
    }
}
