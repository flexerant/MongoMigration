using Flexerant.MongoMigration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    [Migration(1)]
    public class TestMigration_001 : Migration
    {
        public override string Description => "Create the students collection.";

        public override void Migrate(IMongoDatabase db)
        {
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

            var students = db.GetCollection<BsonDocument>("Students");

            students.InsertOne(document);
        }

        public override void MigrateAsTransaction(IMongoDatabase database, IClientSessionHandle session) { }
    }
}
