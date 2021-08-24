using DatabaseMigrations.Configurations;
using DatabaseMigrations.Database;
using DatabaseMigrations.ScriptPreprocessors;
using DatabaseMigrations.ScriptProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;

namespace DatabaseMigrations
{
    public static class DatabaseMigrationsBuilderExtensions
    {
        /// <summary>
        /// Defines the connection to the database to perform migrations on. By default, no migration journal is used. If you want to use a table migration journal, instead call With[Provider]Connection.
        /// </summary>
        /// <param name="connectionFactory">A function that creates a connection to your database.</param>
        /// <param name="owned">
        /// True if DatabaseMigrations should manage the connection. ie, open and dispose it.
        /// </param>
        public static IDatabaseMigrationsBuilder WithConnection(this IDatabaseMigrationsBuilder builder, Func<IServiceProvider, DbConnection> connectionFactory, bool owned = true)
        {
            builder.AddConnectionProvider(connectionFactory, owned);
            builder.WithNoMigrationJournal();

            return builder;
        }

        /// <summary>
        /// Defines the connection to the database to perform migrations on and defines a table migration journal.
        /// </summary>
        /// <param name="connectionFactory">A function that creates a connection to your database.</param>
        /// <param name="owned">
        /// True if DatabaseMigrations should manage the connection. ie, open and dispose it.
        /// </param>
        public static IDatabaseMigrationsBuilder WithSqlServerConnection(this IDatabaseMigrationsBuilder builder, Func<IServiceProvider, DbConnection> connectionFactory, bool owned = true)
        {
            builder.AddConnectionProvider(connectionFactory, owned);
            builder.WithTableMigrationJournal(o => o.ForSqlServer());

            return builder;
        }

        /// <summary>
        /// Defines the connection to the database to perform migrations on and defines a table migration journal.
        /// </summary>
        /// <param name="connectionFactory">A function that creates a connection to your database.</param>
        /// <param name="owned">
        /// True if DatabaseMigrations should manage the connection. ie, open and dispose it.
        /// </param>
        public static IDatabaseMigrationsBuilder WithSqliteConnection(this IDatabaseMigrationsBuilder builder, Func<IServiceProvider, DbConnection> connectionFactory, bool owned = true)
        {
            builder.AddConnectionProvider(connectionFactory, owned);
            builder.WithTableMigrationJournal(o => o.ForSqlite());

            return builder;
        }

        /// <summary>
        /// Defines a custom migration execution order. Default is to execute them in alphabetical order.
        /// </summary>
        public static IDatabaseMigrationsBuilder WithMigrationOrder(this IDatabaseMigrationsBuilder builder, Func<IEnumerable<Migration>, IEnumerable<Migration>> orderFunction)
        {
            builder.Services.AddTransient<IMigrationOrderer, FuncMigrationOrderer>(sp => new FuncMigrationOrderer(orderFunction));

            return builder;
        }

        /// <summary>
        /// Specifies a migration script preprocessor. Preprocessors allow you to mutate the migration script prior to execution. This is useful if you need to do variable substition or other runtime calculations.
        /// </summary>
        public static IDatabaseMigrationsBuilder WithPreprocessor<TScriptPreprocessor>(this IDatabaseMigrationsBuilder builder)
            where TScriptPreprocessor : class, IScriptPreprocessor
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IScriptPreprocessor, TScriptPreprocessor>());
            return builder;
        }

        /// <summary>
        /// Substitute variables in the scripts at runtime with a custom value. This method can be called multiple times for multiple variables.
        /// </summary>
        /// <param name="variableName">The name of variable to replace. This can be whatever you like. You might consider using special characters to avoid accidental replacement.</param>
        /// <param name="value">The value to substitue in at runtime.</param>
        public static IDatabaseMigrationsBuilder WithVariableSubstution(this IDatabaseMigrationsBuilder builder, string variableName, Func<IServiceProvider, string> value)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IScriptPreprocessor, VariableSubstitutionPreprocessor>(sp => new VariableSubstitutionPreprocessor(variableName, () => value(sp))));
            return builder;
        }

        /// <summary>
        /// Specifies a custom migration script source. Allows you to define scripts in code, file servers, etc.
        /// </summary>
        public static IDatabaseMigrationsBuilder WithScriptProvider<TScriptProvider>(this IDatabaseMigrationsBuilder builder)
            where TScriptProvider : class, IScriptProvider
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IScriptProvider, TScriptProvider>());
            return builder;
        }

        /// <summary>
        /// Provide an assembly to search for your scripts that are stored as embedded resources. This method can be called multiple times to specify multiple assemblies. Will find any resource that matches *.sql.
        /// </summary>
        /// <param name="assembly">The assembly to search for scripts.</param>
        /// <returns></returns>
        public static IDatabaseMigrationsBuilder WithEmbeddedScripts(this IDatabaseMigrationsBuilder builder, Assembly assembly) => WithEmbeddedScripts(builder, assembly, n => n.ToLower().EndsWith(".sql"));

        /// <summary>
        /// Provide an assembly to search for your scripts that are stored as embedded resources. This method can be called multiple times to specify multiple assemblies.
        /// </summary>
        /// <param name="assembly">The assembly to search for scripts.</param>
        /// <param name="resourceNameFilter">The resource name filter to limit your migration set</param>
        /// <returns></returns>
        public static IDatabaseMigrationsBuilder WithEmbeddedScripts(this IDatabaseMigrationsBuilder builder, Assembly assembly, Func<string, bool> resourceNameFilter)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IScriptProvider, EmbeddedResourceScriptProvider>(sp => new EmbeddedResourceScriptProvider(assembly, resourceNameFilter)));

            return builder;
        }

        /// <summary>
        /// Specifies a custom migration journal to persist the migration execution history.
        /// </summary>
        public static IDatabaseMigrationsBuilder WithMigrationJournal<TMigrationJournal>(this IDatabaseMigrationsBuilder builder)
            where TMigrationJournal : class, IMigrationJournal
        {
            builder.Services.AddTransient<IMigrationJournal, TMigrationJournal>();
            return builder;
        }

        /// <summary>
        /// Stores the migration execution history in a table in your database. Note you may need to customize the options to support your provider, or use a custom journal if the default table journaling fails.
        /// </summary>
        public static IDatabaseMigrationsBuilder WithTableMigrationJournal(this IDatabaseMigrationsBuilder builder, Action<TableJournalOptions>? config = null)
        {
            config ??= o => { };
            builder.Services.AddTransient<IMigrationJournal, TableMigrationJournal>();
            builder.Services.Configure(config);

            return builder;
        }

        /// <summary>
        /// Disables the migration journal. Meaning each migration script will be ran every time. Recommended that you design your scripts to be idempotent.
        /// </summary>
        public static IDatabaseMigrationsBuilder WithNoMigrationJournal(this IDatabaseMigrationsBuilder builder)
        {
            builder.Services.AddTransient<IMigrationJournal, NullMigrationJournal>();

            return builder;
        }

        private static void AddConnectionProvider(this IDatabaseMigrationsBuilder builder, Func<IServiceProvider, DbConnection> connectionFactory, bool owned)
        {
            if (owned)
                builder.Services.AddTransient<IConnectionProvider>(sp => new OwnedConnectionProvider(() => connectionFactory(sp)));
            else
                builder.Services.AddTransient<IConnectionProvider>(sp => new UnownedConnectionProviderIConnectionProvider(() => connectionFactory(sp)));
        }

    }
}
