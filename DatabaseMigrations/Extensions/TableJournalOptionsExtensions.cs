using DatabaseMigrations.Database;

namespace DatabaseMigrations
{
    public static class TableJournalOptionsExtensions
    {
        public static TableJournalOptions ForSqlServer(this TableJournalOptions options)
        {
            options.CreateJournalTableSql = @"create table [dbo].[MigrationHistory] (
    [Id] varchar(255) not null constraint [PK_MigrationHistory_Id] primary key,
    [Applied] datetime not null
)";
            options.DoesJournalTableExistSql = $"select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'MigrationHistory' and TABLE_SCHEMA = 'dbo'";
            options.InsertEntrySql = "insert into [dbo].[MigrationHistory] ([Id], [Applied]) values ('{0}', '{1}')";
            options.RetrieveEntriesSql = $"select id from [dbo].[MigrationHistory]";

            return options;
        }

        public static TableJournalOptions ForSqlite(this TableJournalOptions options)
        {
            options.CreateJournalTableSql = @"create table [MigrationHistory] (
    [Id] varchar(255) not null constraint [PK_MigrationHistory_Id] primary key,
    [Applied] datetime not null
)";
            options.DoesJournalTableExistSql = $"select count(name) from sqlite_master where type = 'table' and name = 'MigrationHistory'";
            options.InsertEntrySql = "insert into [MigrationHistory] ([Id], [Applied]) values ('{0}', '{1}')";
            options.RetrieveEntriesSql = $"select [id] from [MigrationHistory]";

            return options;
        }

        public static TableJournalOptions ForPostgres(this TableJournalOptions options)
        {
            options.CreateJournalTableSql = @"create table ""dbo"".""MigrationHistory"" (
    ""Id"" character varying(255) not null,
    ""Applied"" timestamp without time zone not null,
    constraint ""PK_MigrationHistory_Id"" primary key (""Id"")
)";
            options.DoesJournalTableExistSql = $"select count(name) from sqlite_master where type = 'table' and name = 'MigrationHistory'";
            options.InsertEntrySql = @"insert into ""dbo"".""MigrationHistory"" (""Id"", ""Applied"") values ('{0}', '{1}')";
            options.RetrieveEntriesSql = $@"select id from ""dbo"".""MigrationHistory""";

            return options;
        }

        //public static TableJournalOptions ForTemplate(this TableJournalOptions options)
        //{
        //    options.CreateJournalTableSql = "";
        //    options.DoesJournalTableSql = "";
        //    options.InsertEntrySql = "";
        //    options.RetrieveEntriesSql = "";

        //    return options;
        //}
    }
}
