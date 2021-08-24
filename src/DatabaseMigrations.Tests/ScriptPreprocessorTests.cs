using DatabaseMigrations.Database;
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

namespace DatabaseMigrations.Tests
{
    [TestClass]
    public class ScriptPreprocessorTests
    {
        [TestMethod]
        public async Task VariableReplacementTest()
        {
            using var conn = Connections.SqliteDbConnFactory(null);
            await conn.OpenAsync();

            string expectedValue = Guid.NewGuid().ToString();

            var services = new ServiceCollection();
            services.AddLogging(l => l.AddConsole());
            services.AddDatabaseMigrations(o =>
            {
                o.WithConnection(sp => conn, owned: false);
                o.WithScriptProvider<TestScriptProvider>();
                o.WithVariableSubstution("$variable$", expectedValue);
            });

            var provider = services.BuildServiceProvider();
            var databaseMigrator = provider.CreateScope().ServiceProvider.GetService<IDatabaseMigrator>();

            await databaseMigrator.ApplyMigrationsAsync(CancellationToken.None);

            var cmd = conn.CreateCommand();
            cmd.CommandText = "select name from test where id = 1";
            var actualValue = (string)cmd.ExecuteScalar();

            actualValue.Should().Be(expectedValue);
        }
    }
}
