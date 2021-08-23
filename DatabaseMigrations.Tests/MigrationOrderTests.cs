using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseMigrations.Tests
{
    [TestClass]
    public class MigrationOrderTests
    {
        [TestMethod]
        public async Task CustomMigrationOrder()
        {
            var executed = false;

            var services = new ServiceCollection();
            services.AddLogging(l => l.AddConsole());
            services.AddDatabaseMigrations(o =>
            {
                o.WithConnection(Connections.SqliteDbConnFactory);
                o.WithScriptProvider<TestScriptProvider>();
                o.WithMigrationOrder(m =>
                {
                    executed = true;
                    return m;
                });
            });

            var provider = services.BuildServiceProvider();
            var databaseMigrator = provider.CreateScope().ServiceProvider.GetService<IDatabaseMigrator>();

            await databaseMigrator.ApplyMigrationsAsync(CancellationToken.None);

            executed.Should().BeTrue();
        }
    }
}
