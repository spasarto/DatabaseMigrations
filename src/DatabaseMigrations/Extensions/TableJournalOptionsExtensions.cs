using DatabaseMigrations.Database;
using System.Data;
using System.Data.Common;

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
            options.InsertEntrySql = "insert into [dbo].[MigrationHistory] ([Id], [Applied]) values (@id, @timestamp)";
            options.RetrieveEntrySql = $"select count(*) from [dbo].[MigrationHistory] where [id] = @id ";

            return options;
        }

        public static TableJournalOptions ForSqlite(this TableJournalOptions options)
        {
            options.CreateJournalTableSql = @"create table [MigrationHistory] (
    [Id] varchar(255) not null constraint [PK_MigrationHistory_Id] primary key,
    [Applied] datetime not null
)";
            options.DoesJournalTableExistSql = $"select count(name) from sqlite_master where type = 'table' and name = 'MigrationHistory'";
            options.InsertEntrySql = "insert into [MigrationHistory] ([Id], [Applied]) values (@id, @timestamp)";
            options.RetrieveEntrySql = $"select count(*) from [MigrationHistory] where [id] = @id ";

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
            options.InsertEntrySql = @"insert into ""dbo"".""MigrationHistory"" (""Id"", ""Applied"") values (@id, @timestamp)";
            options.RetrieveEntrySql = $@"select count(*) from ""dbo"".""MigrationHistory"" where ""id"" = @id";

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

        public static DbParameter With(this DbParameter parameter, string name, DbType type, object value)
        {
            parameter.ParameterName = name;
            parameter.DbType = type;
            parameter.Value = value;

            return parameter;
        }
    }
}
