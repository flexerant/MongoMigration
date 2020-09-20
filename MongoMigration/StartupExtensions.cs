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

namespace Flexerant.MongoMigration
{
    public static class StartupExtensions
    {
        public static void AddMongoMigrations(this IServiceCollection services)
        {
            services.AddMongoMigrations(options => { });
        }

        public static void AddMongoMigrations(this IServiceCollection services, Action<MigrationOptions> options)
        {
            var ass = Assembly.GetCallingAssembly();

            services.Configure<MigrationOptions>(opts =>
            {
                opts.Assemblies.Add(ass);
                options.Invoke(opts);
            });

            services.AddSingleton<IMigrationRunner, MigrationRunner>();
        }

        public static void UseMongoMigrations(this IApplicationBuilder app)
        {
            IMigrationRunner migrationRunner = app.ApplicationServices.GetService<IMigrationRunner>();

            migrationRunner.Run();
        }
    }
}
