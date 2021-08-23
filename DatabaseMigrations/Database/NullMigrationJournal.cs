using System.Threading;
using System.Threading.Tasks;

namespace DatabaseMigrations.Database
{
    public class NullMigrationJournal : IMigrationJournal
    {
        public Task CloseAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task OpenAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<bool> ShouldRunMigrationAsync(Migration migration, CancellationToken cancellationToken) => Task.FromResult(true);

        public Task TrackExecutionAsync(Migration migration, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
