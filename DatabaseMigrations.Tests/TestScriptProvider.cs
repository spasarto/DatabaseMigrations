using DatabaseMigrations.Database;
using DatabaseMigrations.ScriptProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseMigrations.Tests
{
    public class TestScriptProvider : IScriptProvider
    {
        private readonly IEnumerable<Migration> migrations;

        public TestScriptProvider()
        {
            this.migrations = new Migration[] 
            {
                new Migration("1", "create table test(id int, name varchar(255))"),
                new Migration("2", "insert into test (id, name) values (1, '$variable$')")
            };
        }

        public Task<IEnumerable<Migration>> GetScriptsAsync(CancellationToken cancellationToken) => Task.FromResult(migrations);
    }
}
