using DatabaseMigrations.Database;
using DatabaseMigrations.ScriptProviders;
using FluentAssertions;
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
using static DatabaseMigrations.Tests.ScriptProviderTests;

namespace DatabaseMigrations.Tests
{
    [TestClass]
    public class ScriptProviderTests
    {
        [TestMethod]
        public async Task CustomScriptProviderExecutes()
        {
            var expectedValue = 1;

            using var conn = Connections.SqliteDbConnFactory(null);
            await conn.OpenAsync();

            var services = new ServiceCollection();
            services.AddLogging(l => l.AddConsole());
            services.AddDatabaseMigrations(o =>
            {
                o.WithConnection(sp => conn, owned: false);
                o.WithScriptProvider<TestScriptProvider>();
            });

            var provider = services.BuildServiceProvider();
            var databaseMigrator = provider.CreateScope().ServiceProvider.GetService<IDatabaseMigrator>();

            await databaseMigrator.ApplyMigrationsAsync(CancellationToken.None);

            var cmd = conn.CreateCommand();
            cmd.CommandText = "select id from test";
            var actualValue = (long)cmd.ExecuteScalar();

            actualValue.Should().Be(expectedValue);
        }

        [TestMethod]
        public async Task EmbeddedResourceProviderFindsScript()
        {
            var expectedValue = 2;

            using var conn = Connections.SqliteDbConnFactory(null);
            await conn.OpenAsync();
            var services = new ServiceCollection();
            services.AddLogging(l => l.AddConsole());
            services.AddDatabaseMigrations(o =>
            {
                o.WithConnection(sp => conn, owned: false);
                o.WithEmbeddedScripts(Assembly.GetExecutingAssembly(), resourceName => resourceName.Contains("Migration"));
            });

            var provider = services.BuildServiceProvider();
            var databaseMigrator = provider.CreateScope().ServiceProvider.GetService<IDatabaseMigrator>();

            await databaseMigrator.ApplyMigrationsAsync(CancellationToken.None);

            var cmd = conn.CreateCommand();
            cmd.CommandText = "select id from test";
            var actualValue = (long)cmd.ExecuteScalar();

            actualValue.Should().Be(expectedValue);
        }


    }
}
