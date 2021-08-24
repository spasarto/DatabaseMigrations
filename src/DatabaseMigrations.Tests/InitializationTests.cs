using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseMigrations.Tests
{
    [TestClass]
    public class InitializationTests
    {
        [TestMethod]
        public async Task TestSqliteInitialization()
        {
            var services = new ServiceCollection();
            services.AddLogging(l => l.AddConsole());
            services.AddDatabaseMigrations(o =>  o.WithSqliteConnection(Connections.SqliteDbConnFactory));

            var provider = services.BuildServiceProvider();
            var databaseMigrator = provider.CreateScope().ServiceProvider.GetService<IDatabaseMigrator>();

            await databaseMigrator.ApplyMigrationsAsync(CancellationToken.None);
        }

        [TestMethod]
        public async Task TestSqlServerInitialization()
        {
            var services = new ServiceCollection();
            services.AddLogging(l => l.AddConsole());
            services.AddDatabaseMigrations(o => o.WithSqlServerConnection(Connections.SqlServerDbConnFactory));

            var provider = services.BuildServiceProvider();
            var databaseMigrator = provider.CreateScope().ServiceProvider.GetService<IDatabaseMigrator>();

            await databaseMigrator.ApplyMigrationsAsync(CancellationToken.None);
        }
    }
}
