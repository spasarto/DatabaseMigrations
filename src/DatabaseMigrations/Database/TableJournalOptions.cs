using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DatabaseMigrations.Database
{
    public class TableJournalOptions
    {
        public string DoesJournalTableExistSql { get; set; } = "";
        public string CreateJournalTableSql { get; set; } = "";
        public string RetrieveEntrySql { get; set; } = "";
        public string InsertEntrySql { get; set; } = "";

        public Func<Migration, IServiceProvider, Func<DbParameter>, IEnumerable<DbParameter>> Parameters { get; set; } = DefaultParameters;

        public static IEnumerable<DbParameter> DefaultParameters(Migration migration, IServiceProvider serviceProvider, Func<DbParameter> dbParameterFactory)
        {
            yield return dbParameterFactory().With("@id", DbType.String, migration.Id);
            yield return dbParameterFactory().With("@timestamp", DbType.DateTimeOffset, DateTimeOffset.Now);
        }
    }
}
