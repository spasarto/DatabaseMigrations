using DatabaseMigrations.Database;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace JournalWithScope
{
    class TableMigrationEntryCacheWithScope : ITableMigrationEntryCache
    {
        private readonly ISet<(string Id, string Scope)> entries = new HashSet<(string Id, string Scope)>();
        private readonly ISet<(string Id, string Scope)> workingSet = new HashSet<(string Id, string Scope)>();
        private readonly TableJournalOptions tableJournalOptions;
        private readonly ICustomScopeProvider customScopeProvider;

        public TableMigrationEntryCacheWithScope(IOptions<TableJournalOptions> tableJournalOptions, ICustomScopeProvider customScopeProvider)
        {
            this.tableJournalOptions = tableJournalOptions.Value;
            this.customScopeProvider = customScopeProvider;
        }

        public void AddEntryToCacheFromDatabase(DbDataReader reader)
            => entries.Add((reader.GetString(0), reader.GetString(1)));

        //public string FormatEntriesForInsert() => string.Join(Environment.NewLine, entries.Select(m => string.Format(tableJournalOptions.InsertEntrySql, m.Id, m.Scope, DateTimeOffset.Now)));
        public string FormatEntriesForInsert()
        {
            var sql = string.Join(Environment.NewLine, workingSet.Select(m => string.Format(tableJournalOptions.InsertEntrySql, m.Id, m.Scope, DateTimeOffset.Now)));
            entries.UnionWith(workingSet);
            workingSet.Clear();
            return sql;
        }
        public bool ShouldRun(Migration migration) => !entries.Contains((migration.Id, customScopeProvider.Scope)) && !workingSet.Contains((migration.Id, customScopeProvider.Scope));

        public void TrackExecution(Migration migration) => workingSet.Add((migration.Id, customScopeProvider.Scope));
    }
}
