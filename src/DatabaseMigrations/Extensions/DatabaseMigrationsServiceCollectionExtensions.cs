using DatabaseMigrations.Configurations;
using DatabaseMigrations.Database;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data.Common;
using System.Linq;

namespace DatabaseMigrations
{
    public static class DatabaseMigrationsServiceCollectionExtensions
    {
        /// <summary>
        /// Adds database migrations to the application with default options. Scripts will be executed in alphabetical order.  No migration journal will be created by default. If you would like a table to track script execution history, call the overload with the configure action to configure it for your database provider.
        /// </summary>
        public static IServiceCollection AddDatabaseMigrations(this IServiceCollection services, Func<IServiceProvider, DbConnection> connectionFactory)
        {
            return AddDatabaseMigrations(services, m => { m.WithConnection(connectionFactory); });
        }

        /// <summary>
        /// Adds database migrations to the application. Use the configuration action to customize the migration process.
        /// </summary>
        public static IServiceCollection AddDatabaseMigrations(this IServiceCollection services, Action<IDatabaseMigrationsBuilder> configure)
        {
            services.AddTransient<IDatabaseMigrator, DatabaseMigrator>();
            services.AddTransient<ISqlCommandSegmentor, RegexSqlCommandSegmentor>();
            services.AddTransient<IMigrationOrderer>(sp => new FuncMigrationOrderer(m => m.OrderBy(x => x.Id)));
            services.AddLogging();

            var builder = new DatabaseMigrationsBuilder(services);
            builder.WithTableMigrationJournal(o => { });

            configure(builder);
            return services;
        }

    }
}
