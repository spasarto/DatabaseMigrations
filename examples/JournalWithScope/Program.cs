using Microsoft.Extensions.Hosting;
using System;
using DatabaseMigrations;
using Microsoft.Data.Sqlite;
using JournalWithScope;
using Microsoft.Extensions.DependencyInjection;
using DatabaseMigrations.Database;
using Microsoft.Extensions.Logging;
using System.Reflection;

await new HostBuilder()
        .ConfigureServices(services =>
        {
            services.AddDatabaseMigrations(o =>
            {
                o.WithConnection(sp => new SqliteConnection("Data Source=:memory:"));
                o.WithTableMigrationJournal(t => t.ForCustomScope());
                o.WithVariableSubstution("$username$", Environment.UserName);
                o.WithPreprocessor<ScopeVariablePreprocessor>();
                o.WithEmbeddedScripts(Assembly.GetExecutingAssembly());
            });
            services.AddSingleton<ICustomScopeProvider, CustomScopeProvider>();
            services.AddHostedService<RunMigrationsService>();
        })
        .ConfigureLogging(o => o.AddConsole())
        .RunConsoleAsync();

