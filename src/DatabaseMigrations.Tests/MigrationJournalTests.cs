using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseMigrations.Tests
{
    [TestClass]
    public class MigrationJournalTests
    {
        [TestMethod]
        public async Task MigrationIsSkipped()
        {
            var services = new ServiceCollection();
            services.AddLogging(l => l.AddConsole());
            services.AddDatabaseMigrations(o =>
            {
                o.WithConnection(Connections.SqliteDbConnFactory);
                o.WithScriptProvider<TestScriptProvider>();
                o.WithMigrationJournal<TestMigrationJournal>();
            });

            var provider = services.BuildServiceProvider();
            var databaseMigrator = provider.CreateScope().ServiceProvider.GetService<IDatabaseMigrator>();

            await databaseMigrator.ApplyMigrationsAsync(CancellationToken.None);
            // exception will be thrown if migration is saved.
        }
    }
}
