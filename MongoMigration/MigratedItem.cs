using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flexerant.MongoMigration
{
    public class MigratedItem
    {
        [BsonId]
        public int MigrationNumber { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Type { get; set; }
        public string Assembly { get; set; }
        public string Description { get; set; }
    }
}
