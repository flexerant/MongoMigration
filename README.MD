# Flexerant.MongoMigration

Flexerant.MongoMigration is a .NET Core utility for migrating MongoDB databases and schemas and uses the [MongoDB .NET driver](https://mongodb.github.io/mongo-csharp-driver/). Unlike other migration implementations, this library only offers forward-only migrations: the rationale being that all database changes should be auditable and forward-only migrations ensure that every change is tracked, even changes back to previous states. With this library, migrations occur during startup and support dependency injection.

# Installation

[![nuget](https://img.shields.io/nuget/v/Flexerant.MongoMigration?label=nuget)](https://www.nuget.org/packages/Flexerant.MongoMigration/)

`PM> Install-Package Flexerant.MongoMigration`

# Setup

```csharp
public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        ...

        IMongoClient mongoClient = new MongoClient(this.Configuration["MongoDB:ConnectionString"]);

        services.AddSingleton<IMongoDatabase, MongoDatabaseBase>(x => (MongoDatabaseBase)mongoClient.GetDatabase(this.Configuration["MongoDB:Database"]));
        services.AddMongoMigrations();

        ...
    }

    ...

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        ...

        app.UseMongoMigrations();

        ...
    }
}

```

# Migrations

Each migration inherits `Flexerant.MongoMigration.Migration`. The migration number is controlled by the `[Migration]` attribute. Successful migrations result a record saved to the database, whereas failures roll-back the failed migration.

```csharp
[Migration(1)]
public class Migration_001 : Migration
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
}
```

```csharp
[Migration(2)]
public class Migration_002 : Migration
{
    public override string Description => "Create class";

    public override void Migrate(IMongoDatabase db)
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

        var classes = db.GetCollection<BsonDocument>("Classes");

        classes.InsertOne(document);
    }
}
```