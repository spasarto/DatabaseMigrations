using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseMigrations.Database
{
    public class TableMigrationJournal : IMigrationJournal
    {
        protected readonly IConnectionProvider connectionProvider;
        protected readonly ILogger<TableMigrationJournal> logger;
        private readonly IServiceProvider serviceProvider;
        protected readonly TableJournalOptions tableJournalOptions;

        public TableMigrationJournal(IConnectionProvider connectionProvider,
                                     IOptions<TableJournalOptions> tableJournalOptions,
                                     ILogger<TableMigrationJournal> logger,
                                     IServiceProvider serviceProvider)
        {
            this.connectionProvider = connectionProvider;
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.tableJournalOptions = tableJournalOptions.Value;
        }

        public virtual async Task OpenAsync(CancellationToken cancellationToken)
        {
            var conn = this.connectionProvider.Get();
            var cmd = conn.CreateCommand();

            bool tableExists = await DoesTableExistsAsync(cmd, cancellationToken);

            if (!tableExists)
                await CreateTableAsync(cmd, cancellationToken);
        }

        public virtual Task CloseAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public virtual async Task<bool> ShouldRunMigrationAsync(Migration migration, CancellationToken cancellationToken)
        {
            var conn = this.connectionProvider.Get();
            var cmd = conn.CreateCommand();

            try
            {
                cmd.CommandText = this.tableJournalOptions.RetrieveEntrySql;
                cmd.Parameters.AddRange(this.tableJournalOptions.Parameters(migration, serviceProvider, () => cmd.CreateParameter()).ToArray());

                object val = await cmd.ExecuteScalarAsync(cancellationToken);

                return !Convert.ToBoolean(val);
            }
            catch (Exception ex)
            {
                var e = new TableJournalException("Failed to check migration journal table.", cmd.CommandText, ex);
                logger.LogError(e, "");
                throw e;
            }
        }

        public virtual async Task TrackExecutionAsync(Migration migration, CancellationToken cancellationToken)
        {
            var conn = this.connectionProvider.Get();
            var cmd = conn.CreateCommand();

            try
            {
                cmd.CommandText = this.tableJournalOptions.InsertEntrySql;
                cmd.Parameters.AddRange(this.tableJournalOptions.Parameters(migration, serviceProvider, () => cmd.CreateParameter()).ToArray());

                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                var e = new TableJournalException("Failed to update migration journal table.", cmd.CommandText, ex);
                logger.LogError(e, "");
                throw e;
            }
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
