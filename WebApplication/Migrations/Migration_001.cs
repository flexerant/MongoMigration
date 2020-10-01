using Flexerant.MongoMigration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Data
{
    [Migration(1)]
    public class Migration_001 : Migration
    {
        public override string Description => "Create student";

        public override void Migrate(IMongoDatabase database) { }

        public override void MigrateAsTransaction(IMongoDatabase database, IClientSessionHandle session)
        {
            database.CreateCollection(session, "Students");
            var students = database.GetCollection<BsonDocument>("Students");
          
            session.StartTransaction();

            var document = new BsonDocument { { "student_id", 10000 },
                {
                    "scores",
                    new BsonArray {
                        new BsonDocument { { "type", "exam" }, { "score", 88.12334193287023 } },
                        new BsonDocument { { "type", "quiz" }, { "score", 74.92381029342834 } },
                        new BsonDocument { { "type", "homework" }, { "score", 89.97929384290324 } },
                        new BsonDocument { { "type", "homework" }, { "score", 82.12931030513218 } }
                    }
                }, { "class_id", 480 }
            };

            students.InsertOne(session, document);
        }
    }
}
