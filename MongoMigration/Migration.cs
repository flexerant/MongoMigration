using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MongoMigration
{
    public abstract class Migration
    {
        public abstract string Description { get; }
        internal MigrationAttribute MigrationAttribute => this.GetType().GetCustomAttribute<MigrationAttribute>();

        public abstract void Up(IMongoDatabase db);
        //public abstract void Down(IMongoDatabase db);
    }
}
