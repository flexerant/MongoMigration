# Flexerant.MongoMigration

[![nuget](https://img.shields.io/nuget/v/Flexerant.MongoMigration?label=nuget)](https://www.nuget.org/packages/Flexerant.MongoMigration/)

Flexerant.MongoMigration is a .NET Standard utility for migrating MongoDB databases and schemas, based on the [MongoDB .NET driver](https://mongodb.github.io/mongo-csharp-driver/). Unlike other migration implementations, this library only offers forward-only migrations: the rationale being that all database changes should be auditable and forward-only migrations ensure that every change is tracked. To revert back to a previous state, simply write a migration that undoes the previous one(s). With this library, migrations occur during startup and support dependency injection.

## Motivation

This project was born out of a need to automate MongoDB migrations when deployed. Simplicity and ease of use were key.

## Installation

Intall from nuget.

`PM> Install-Package Flexerant.MongoMigration`

## Setup

First, start by adding `AddMongoMigrations` extension method to the `IServiceCollection` object;

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        ...

        IMongoClient mongoClient = new MongoClient(this.Configuration["MongoDB:ConnectionString"]);

        services.AddSingleton<IMongoDatabase, MongoDatabaseBase>(x => (MongoDatabaseBase)mongoClient.GetDatabase(this.Configuration["MongoDB:Database"]));
        services.AddMongoMigrations();

        ...
    }
}

```
The `AddMongoMigrations` extension method takes an optional `MigrationOptions` delegate. 

| Property          | Description |
| -------------     | ------------- |
| `Assemblies`      | An optional collection of assemblies containing migrations. By default the calling assembly is added to the collection. |
| `MongoDatabase`   | An optional database to use for the migrations. By default the injected IMongoDatabase service is used. |

Secondly, add the `UseMongoMigrations` extension method to the `IApplicationBuilder` object.

```csharp
public class Startup
{
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        ...

        app.UseMongoMigrations();

        ...
    }
}

```

That's it! Migrations will automatically be executed on start up.

## Usage

Each migration inherits `Flexerant.MongoMigration.Migration`. The migration number is controlled by the `[Migration]` attribute. Successful migrations result a transaction record saved to the database, whereas failures roll-back the failed migration. Dependency injection is supported so dependencies can be accessed as part of the migration process.

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

The `Migrate` methods provides access to the `IMongoDatabase` object and therefore any operation supported by `IMongoDatabase` is possible.

## License

![GitHub](https://img.shields.io/github/license/flexerant/MongoMigration)