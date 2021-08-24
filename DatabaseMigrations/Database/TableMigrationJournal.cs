using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseMigrations.Database
{
    public class TableMigrationJournal : IMigrationJournal
    {
        protected readonly IConnectionProvider connectionProvider;
        protected readonly ILogger<TableMigrationJournal> logger;
        private readonly ITableMigrationEntryCache tableMigrationEntryCache;
        protected readonly TableJournalOptions tableJournalOptions;

        public TableMigrationJournal(IConnectionProvider connectionProvider,
                                     IOptions<TableJournalOptions> tableJournalOptions,
                                     ILogger<TableMigrationJournal> logger,
                                     ITableMigrationEntryCache tableMigrationEntryCache)
        {
            this.connectionProvider = connectionProvider;
            this.logger = logger;
            this.tableMigrationEntryCache = tableMigrationEntryCache;
            this.tableJournalOptions = tableJournalOptions.Value;
        }

        public virtual async Task OpenAsync(CancellationToken cancellationToken)
        {
            var conn = await this.connectionProvider.GetAsync(cancellationToken);
            var cmd = conn.CreateCommand();

            bool tableExists = await DoesTableExistsAsync(cmd, cancellationToken);

            if (!tableExists)
                await CreateTableAsync(cmd, cancellationToken);

            cmd.CommandText = tableJournalOptions.RetrieveEntriesSql;

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                tableMigrationEntryCache.AddEntryToCacheFromDatabase(reader);
            }
        }

        public virtual async Task CloseAsync(CancellationToken cancellationToken)
        {
            var conn = await this.connectionProvider.GetAsync(cancellationToken);
            var cmd = conn.CreateCommand();

            try
            {
                cmd.CommandText = this.tableMigrationEntryCache.FormatEntriesForInsert();

                if (!string.IsNullOrEmpty(cmd.CommandText))
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                var e = new TableJournalException("Failed to update migration journal table.", cmd.CommandText, ex);
                logger.LogError(e, "");
                throw e;
            }
        }

        public virtual Task<bool> ShouldRunMigrationAsync(Migration migration, CancellationToken cancellationToken)
        {
            bool exists = this.tableMigrationEntryCache.ShouldRun(migration);

            return Task.FromResult(exists);
        }

        public virtual Task TrackExecutionAsync(Migration migration, CancellationToken cancellationToken)
        {
            this.tableMigrationEntryCache.TrackExecution(migration);

            return Task.CompletedTask;
        }

        protected virtual async Task<bool> DoesTableExistsAsync(DbCommand cmd, CancellationToken cancellationToken)
        {
            try
            {
                cmd.CommandText = tableJournalOptions.DoesJournalTableExistSql;
                object result = await cmd.ExecuteScalarAsync(cancellationToken);
                return Convert.ToBoolean(result);
            }
            catch (Exception ex)
            {
                var e = new TableJournalException("Failed to check for migration journal table.", cmd.CommandText, ex);
                logger.LogError(e, "");
                throw e;
            }
        }

        protected virtual async Task CreateTableAsync(DbCommand cmd, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Journal table does not exist. Creating now.");
                cmd.CommandText = tableJournalOptions.CreateJournalTableSql;

                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                var e = new TableJournalException("Failed to create the journal table.", cmd.CommandText, ex);
                logger.LogError(e, "");
                throw e;
            }
        }
    }

    public class TableJournalException : Exception
    {
        public TableJournalException(string message, string sql, Exception innerException)
            : base($"{message} This is most likely caused due to your database provider using an unexpected sql syntax. You can customize the table journal options by calling {nameof(DatabaseMigrationsBuilderExtensions.WithTableMigrationJournal)}. Or, you can specify a custom migration journal by calling {nameof(DatabaseMigrationsBuilderExtensions.WithMigrationJournal)}. For reference, the SQL we tried to execute was:{Environment.NewLine}{sql}", innerException) { }
    }
}
