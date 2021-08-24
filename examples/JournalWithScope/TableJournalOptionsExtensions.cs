using DatabaseMigrations.Database;

namespace JournalWithScope
{
    public static class TableJournalOptionsExtensions
    {
        public static TableJournalOptions ForCustomScope(this TableJournalOptions options)
        {
            options.CreateJournalTableSql = @"create table [MigrationHistory] (
    [Id] varchar(255) not null,
    [Scope] varchar(255) null,
    [Applied] datetime not null,
    PRIMARY KEY(Id, Scope)
)";
            options.DoesJournalTableExistSql = $"select count(name) from sqlite_master where type = 'table' and name = 'MigrationHistory'";
            options.InsertEntrySql = "insert into [MigrationHistory] ([Id], [Scope], [Applied]) values ('{0}', '{1}', '{2}')";
            options.RetrieveEntriesSql = $"select [id], [Scope] from [MigrationHistory]";
            return options;
        }
    }
}
