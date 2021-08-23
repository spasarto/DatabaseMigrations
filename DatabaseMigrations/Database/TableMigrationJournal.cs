using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseMigrations.Database
{
    public class TableMigrationJournal : IMigrationJournal
    {
        private readonly IConnectionProvider connectionProvider;
        private readonly ILogger<TableMigrationJournal> logger;
        private readonly ConcurrentDictionary<string, object?> entries;
        private readonly TableJournalOptions tableJournalOptions;

        public TableMigrationJournal(IConnectionProvider connectionProvider,
                                     IOptions<TableJournalOptions> tableJournalOptions,
                                     ILogger<TableMigrationJournal> logger)
        {
            this.connectionProvider = connectionProvider;
            this.logger = logger;
            this.entries = new ConcurrentDictionary<string, object?>();
            this.tableJournalOptions = tableJournalOptions.Value;
        }

        public async Task OpenAsync(CancellationToken cancellationToken)
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
                var id = reader.GetString(0);
                this.entries.TryAdd(id, null);
            }
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            if (entries.Count == 0)
                return;

            var conn = await this.connectionProvider.GetAsync(cancellationToken);
            var cmd = conn.CreateCommand();

            try
            {
                var now = DateTimeOffset.Now;

                cmd.CommandText = string.Join(Environment.NewLine, entries.Keys
                                                                  .Select(m => string.Format(tableJournalOptions.InsertEntrySql, m, now)));

                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                var e = new TableJournalException("Failed to update migration journal table.", cmd.CommandText, ex);
                logger.LogError(e, "");
                throw e;
            }
        }

        public Task<bool> ShouldRunMigrationAsync(Migration migration, CancellationToken cancellationToken)
        {
            bool exists = entries.ContainsKey(migration.Id);

            return Task.FromResult(exists);
        }

        public Task TrackExecutionAsync(Migration migration, CancellationToken cancellationToken)
        {
            entries.TryAdd(migration.Id, null);

            return Task.CompletedTask;
        }

        private async Task<bool> DoesTableExistsAsync(DbCommand cmd, CancellationToken cancellationToken)
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

        private async Task CreateTableAsync(DbCommand cmd, CancellationToken cancellationToken)
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
