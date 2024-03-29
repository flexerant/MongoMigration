﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Reflection;

namespace Flexerant.MongoMigration
{
    public static class StartupExtensions
    {
        public static void AddMongoMigrations(this IServiceCollection services, Action<MigrationOptions> options = null)
        {
            var ass = Assembly.GetCallingAssembly();

            services.Configure<MigrationOptions>(opts =>
            {
                opts.Assemblies.Add(ass);

                if (options != null) options.Invoke(opts);
            });
        }

        public static void UseMongoMigrations(this IApplicationBuilder app)
        {
            var sp = app.ApplicationServices;
            var options = sp.GetService<IOptions<MigrationOptions>>();
            var logger = sp.GetService<ILogger<MigrationRunner>>();
            IMongoDatabase db;

            if (options.Value.MongoDatabase == null)
            {
                db = sp.GetService<IMongoDatabase>();
            }
            else
            {
                db = options.Value.MongoDatabase;
            }

            if (db == null)
            {
                throw new InvalidOperationException($"No {typeof(IMongoDatabase).Name} instance was found. Either add it to the {typeof(IServiceCollection).Name} instance or set the {typeof(MigrationOptions).Name} delegate in {nameof(AddMongoMigrations)}");
            }

            var migration = new MigrationRunner(sp, options, db, logger);

            migration.Run();
        }
    }
}
