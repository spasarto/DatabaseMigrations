//using DatabaseMigrations.Database;
//using Microsoft.Extensions.Options;
//using System.Collections.Generic;
//using System.Data.Common;

//namespace JournalWithScope
//{
//    class TableMigrationEntryCacheWithScope : TableMigrationEntryCache<(string Id, string Scope)>
//    {
//        private readonly ICustomScopeProvider customScopeProvider;

//        public TableMigrationEntryCacheWithScope(IOptions<TableJournalOptions> tableJournalOptions, ICustomScopeProvider customScopeProvider)
//            : base(tableJournalOptions)
//        {
//            this.customScopeProvider = customScopeProvider;
//        }

//        protected override (string, string) FromMigration(Migration migration) => (migration.Id, customScopeProvider.Scope);

//        protected override (string, string) FromMigration(DbDataReader reader) => (reader.GetString(0), reader.GetString(1));

//        protected override IEnumerable<object> GetColumnValues((string Id, string Scope) entity)
//        {
//            yield return entity.Id;
//            yield return entity.Scope;
//        }
//    }
//}
