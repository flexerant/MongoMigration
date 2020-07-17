using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace MongoMigration
{
    public static class StartupExtensions
    {
        public static void AddMongoMigrations(this IServiceCollection services, Action<MigrationOptions> options)
        {
            var ass = Assembly.GetCallingAssembly();

            services.Configure<MigrationOptions>(opts =>
            {
                options.Invoke(opts);

                if (!opts.Assemblies.Any())
                {
                    opts.Assemblies.Add(ass);
                }

                if (opts.Logger == null)
                {
                    opts.Logger = services.BuildServiceProvider().GetService<ILogger>();
                }

                if (opts.MongoDatabase == null)
                {
                    opts.MongoDatabase = services.BuildServiceProvider().GetService<IMongoDatabase>();
                }

                if (opts.MongoDatabase == null)
                {
                    throw new MigrationException($"The dependency '{typeof(IMongoDatabase).FullName}' could not be found.");
                }
            });

            services.TryAddSingleton<IMigrationRunner, MigrationRunner>();
        }

        public static void UseMongoMigrations(this IApplicationBuilder app)
        {
            IMigrationRunner migrationRunner = app.ApplicationServices.GetService<IMigrationRunner>();

            migrationRunner.Run();
        }
    }
}
