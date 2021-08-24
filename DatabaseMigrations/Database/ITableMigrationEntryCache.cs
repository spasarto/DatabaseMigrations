using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace DatabaseMigrations.Database
{
    /// <summary>
    /// Manages the cache for the entities that represent the migrations. Defines uniqueness for the migrations and controls serialization.
    /// </summary>
    public interface ITableMigrationEntryCache
    {
        /// <summary>
        /// Reads a single entry from the database and caches it for later use, to help determine if we should execution the migration.
        /// </summary>
        void AddEntryToCacheFromDatabase(DbDataReader reader);
        /// <summary>
        /// Serializes the migration entries to be inserted into the database.
        /// </summary>
        string FormatEntriesForInsert();
        /// <summary>
        /// Updates the cache, marking the migration as executed.
        /// </summary>
        void TrackExecution(Migration migration);
        /// <summary>
        /// Determines if the migration should be executed.
        /// </summary>
        bool ShouldRun(Migration migration);
    }

    public class TableMigrationEntryIdCache : ITableMigrationEntryCache
    {
        private readonly ISet<string> entries = new HashSet<string>();
        private readonly TableJournalOptions tableJournalOptions;

        public TableMigrationEntryIdCache(IOptions<TableJournalOptions> tableJournalOptions)
        {
            this.tableJournalOptions = tableJournalOptions.Value;
        }

        public void AddEntryToCacheFromDatabase(DbDataReader reader) => entries.Add(reader.GetString(0));
        public string FormatEntriesForInsert() => string.Join(Environment.NewLine, entries.Select(m => string.Format(tableJournalOptions.InsertEntrySql, m, DateTimeOffset.Now)));
        public bool ShouldRun(Migration migration) => !entries.Contains(migration.Id);
        public void TrackExecution(Migration migration) => entries.Add(migration.Id);
    }

}
