using System.Threading;
using System.Threading.Tasks;

namespace DatabaseMigrations.Database
{
    /// <summary>
    /// Represents a storage location for the script execution history.
    /// </summary>
    public interface IMigrationJournal
    {
        /// <summary>
        /// Initializes the connection to your data store. You can load up the entries into memory to make script execution quicker.
        /// </summary>
        Task OpenAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Commits any changes to the migration execution history. Recommended you save your changes here in bulk instead of individually in TrackExecutionAsync.
        /// </summary>
        Task CloseAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Returns true if the migration should be ran.
        /// </summary>
        Task<bool> ShouldRunMigrationAsync(Migration migration, CancellationToken cancellationToken);
        /// <summary>
        /// Updates the journal that the migration was ran successfully.
        /// </summary>
        Task TrackExecutionAsync(Migration migration, CancellationToken cancellationToken);
    }
}
