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

    public abstract class TableMigrationEntryCache<TEntity> : ITableMigrationEntryCache
    {
        protected readonly ISet<TEntity> entries = new HashSet<TEntity>();
        protected readonly ISet<TEntity> workingSet = new HashSet<TEntity>();
        protected readonly TableJournalOptions tableJournalOptions;

        public TableMigrationEntryCache(IOptions<TableJournalOptions> tableJournalOptions)
        {
            this.tableJournalOptions = tableJournalOptions.Value;
        }

        public void AddEntryToCacheFromDatabase(DbDataReader reader) => entries.Add(FromMigration(reader));
        
        public string FormatEntriesForInsert()
        {
            var sql = string.Join(Environment.NewLine, workingSet.Select(m => string.Format(tableJournalOptions.InsertEntrySql, GetColumnValues(m).Append(DateTimeOffset.Now).ToArray())));
            entries.UnionWith(workingSet);
            workingSet.Clear();
            return sql;
        }

        public bool ShouldRun(Migration migration) => !entries.Contains(FromMigration(migration)) 
                                                   && !workingSet.Contains(FromMigration(migration));
        
        public void TrackExecution(Migration migration) => workingSet.Add(FromMigration(migration));

        protected abstract TEntity FromMigration(Migration migration);
        protected abstract TEntity FromMigration(DbDataReader reader);
        protected abstract IEnumerable<object> GetColumnValues(TEntity entity);
    }

    public class TableMigrationEntryIdCache : TableMigrationEntryCache<string>
    {
        public TableMigrationEntryIdCache(IOptions<TableJournalOptions> tableJournalOptions) 
            : base(tableJournalOptions)
        {
        }

        protected override string FromMigration(Migration migration) => migration.Id;

        protected override string FromMigration(DbDataReader reader) => reader.GetString(0);

        protected override IEnumerable<object> GetColumnValues(string entity) { yield return entity; }
    }
}
