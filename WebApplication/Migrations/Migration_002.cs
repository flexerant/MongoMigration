using MongoDB.Bson;
using MongoDB.Driver;
using MongoMigration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Data
{
    [Migration(2)]
    public class Migration_002 : Migration
    {
        public override string Description => "Rename 'scores' field to 'Scores'.";

        public override void Up(IMongoDatabase db)
        {
            var filter = Builders<BsonDocument>.Filter.Empty;
            var update = Builders<BsonDocument>.Update.Rename(x => x["scores"], "Scores");
            var students = db.GetCollection<BsonDocument>("Students");

            students.UpdateMany(filter, update);
        }
    }
}
