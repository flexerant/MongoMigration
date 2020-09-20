using Flexerant.MongoMigration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;

namespace TestMigrations
{
    [Migration(2)]
    public class Migration_002 : Migration
    {
        public override string Description => "Create class";

        public override void Up(IMongoDatabase db)
        {
            var document = new BsonDocument { { "class_id", 480 },
                {
                    "schedule",
                    new BsonArray {
                        new BsonDocument { { "day", "Monday" }, { "start-time", "8:00AM" }, { "duration", 1.5 } },
                        new BsonDocument { { "day", "Wednesday" }, { "start-time", "9:00AM" }, { "duration", 1.5 } },
                        new BsonDocument { { "day", "Friday" }, { "start-time", "1:00PM" }, { "duration", 3 } },
                    }
                }
            };

            var students = db.GetCollection<BsonDocument>("Classes");

            students.InsertOne(document);
        }
    }
}
