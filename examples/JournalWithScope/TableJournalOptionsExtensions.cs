using DatabaseMigrations;
using DatabaseMigrations.Database;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Linq;

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
            options.InsertEntrySql = "insert into [MigrationHistory] ([Id], [Scope], [Applied]) values (@id, @scope, @timestamp)";
            options.RetrieveEntrySql = $"select count(*) from [MigrationHistory] where [id] = @id and [Scope] = @scope";
            options.Parameters = (migration, serviceProvider, dbParameterFactory) =>
                    TableJournalOptions.DefaultParameters(migration, serviceProvider, dbParameterFactory)
                                       .Append(dbParameterFactory().With("@scope", DbType.String, serviceProvider.GetService<ICustomScopeProvider>().Scope));
            return options;
        }
    }
}
