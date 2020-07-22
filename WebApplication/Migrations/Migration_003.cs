using Flexerant.MongoMigration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Data
{
    [Migration(3)]
    public class Migration_003 : Migration
    {
        private readonly IHelloService _helloService;

        public Migration_003(IHelloService helloService)
        {
            _helloService = helloService;
        }

        public override string Description => _helloService.Greeting;

        public override void Up(IMongoDatabase db)
        {
            var filter = Builders<BsonDocument>.Filter.Empty;
            var updateClientIdTemp = Builders<BsonDocument>.Update.Set(x => x["class_id_temp"], string.Empty);
            var students = db.GetCollection<BsonDocument>("Students");
            var allStudents = students.Find(filter).ToList();

            students.UpdateMany(filter, updateClientIdTemp);

            List<UpdateOneModel<BsonDocument>> updateAll = new List<UpdateOneModel<BsonDocument>>();

            foreach (var student in allStudents)
            {
                var studentFilter = Builders<BsonDocument>.Filter.Where(x => x["_id"] == student["_id"]);
                var studentUpdate = Builders<BsonDocument>.Update.Set(x => x["class_id_temp"], student["class_id"].AsInt32.ToString());

                updateAll.Add(new UpdateOneModel<BsonDocument>(studentFilter, studentUpdate));
            }

            students.BulkWrite(updateAll);
            updateAll = new List<UpdateOneModel<BsonDocument>>();

            foreach (var student in allStudents)
            {
                var studentFilter = Builders<BsonDocument>.Filter.Where(x => x["_id"] == student["_id"]);
                var studentUpdate = Builders<BsonDocument>.Update.Unset(x => x["class_id"]);

                updateAll.Add(new UpdateOneModel<BsonDocument>(studentFilter, studentUpdate));
            }

            students.BulkWrite(updateAll);
            updateAll = new List<UpdateOneModel<BsonDocument>>();

            foreach (var student in allStudents)
            {
                var studentFilter = Builders<BsonDocument>.Filter.Where(x => x["_id"] == student["_id"]);
                var studentUpdate = Builders<BsonDocument>.Update.Rename(x => x["class_id_temp"], "class_id");

                updateAll.Add(new UpdateOneModel<BsonDocument>(studentFilter, studentUpdate));
            }

            students.BulkWrite(updateAll);
            updateAll = new List<UpdateOneModel<BsonDocument>>();

            foreach (var student in allStudents)
            {
                var studentFilter = Builders<BsonDocument>.Filter.Where(x => x["_id"] == student["_id"]);
                var studentUpdate = Builders<BsonDocument>.Update.Unset(x => x["class_id_temp"]);

                updateAll.Add(new UpdateOneModel<BsonDocument>(studentFilter, studentUpdate));
            }
        }
    }
}
