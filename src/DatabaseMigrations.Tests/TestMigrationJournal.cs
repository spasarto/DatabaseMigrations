using DatabaseMigrations.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseMigrations.Tests
{
    public class TestMigrationJournal : IMigrationJournal
    {
        public Task CloseAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task OpenAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<bool> ShouldRunMigrationAsync(Migration migration, CancellationToken cancellationToken) => Task.FromResult(false);

        public Task TrackExecutionAsync(Migration migration, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
