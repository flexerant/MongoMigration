using Flexerant.MongoMigration;
using MongoDB.Bson;
using MongoDB.Driver;
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

        public override void Migrate(IMongoDatabase database) { }

        public override void MigrateAsTransaction(IMongoDatabase database, IClientSessionHandle session)
        {
            var filter = Builders<BsonDocument>.Filter.Empty;
            var update = Builders<BsonDocument>.Update.Rename(x => x["scores"], "Scores");
            var students = database.GetCollection<BsonDocument>("Students");

            session.StartTransaction();

            students.UpdateMany(session, filter, update);
        }
    }
}
