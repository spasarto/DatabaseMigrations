using DatabaseMigrations.Database;
using DatabaseMigrations.ScriptPreprocessors;
using DatabaseMigrations.ScriptProviders;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseMigrations
{
    public interface IDatabaseMigrator
    {
        /// <summary>
        /// Executes the located scripts to apply the migrations. If a migration journal is used, only the scripts that need to be ran will be executed.
        /// </summary>
        Task ApplyMigrationsAsync(CancellationToken cancellationToken);
    }

    public class DatabaseMigrator : IDatabaseMigrator
    {
        private readonly IEnumerable<IScriptProvider> scriptProviders;
        private readonly IEnumerable<IScriptPreprocessor> scriptPreprocessors;
        private readonly ISqlCommandSegmentor sqlCommandSegmentor;
        private readonly IConnectionProvider connectionProvider;
        private readonly IMigrationJournal migrationJournal;
        private readonly IMigrationOrderer migrationOrderer;
        private readonly ILogger<DatabaseMigrator> logger;

        public DatabaseMigrator(IEnumerable<IScriptProvider> scriptProviders,
                                IEnumerable<IScriptPreprocessor> scriptPreprocessors,
                                ISqlCommandSegmentor sqlCommandSegmentor,
                                IConnectionProvider connectionProvider,
                                IMigrationJournal migrationJournal,
                                IMigrationOrderer migrationOrderer,
                                ILogger<DatabaseMigrator> logger)
        {
            this.scriptProviders = scriptProviders;
            this.scriptPreprocessors = scriptPreprocessors;
            this.sqlCommandSegmentor = sqlCommandSegmentor;
            this.connectionProvider = connectionProvider;
            this.migrationJournal = migrationJournal;
            this.migrationOrderer = migrationOrderer;
            this.logger = logger;
        }

        public async Task ApplyMigrationsAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Checking migration journal");
            await migrationJournal.OpenAsync(cancellationToken);

            var conn = await connectionProvider.GetAsync(cancellationToken);
            var cmd = conn.CreateCommand();

            this.logger.LogInformation("Processing scripts");
            await ProcessScriptsAsync(cmd, cancellationToken);

            this.logger.LogInformation("Updating migration journal");
            await migrationJournal.CloseAsync(cancellationToken);
        }

        private async Task ProcessScriptsAsync(DbCommand cmd, CancellationToken cancellationToken)
        {
            bool executedAnyScript = false;

            var scripts = await HydrateMigrationScripts(cancellationToken);

            foreach (var script in scripts)
            {
                executedAnyScript = true;
                cancellationToken.ThrowIfCancellationRequested();

                if (await migrationJournal.ShouldRunMigrationAsync(script, cancellationToken))
                {
                    this.logger.LogInformation($"Executing script '{script.Id}'");
                    var processedScript = await PreprocessScriptAsync(script.Contents, cancellationToken);

                    var commandSegments = await sqlCommandSegmentor.SegementScriptAsync(processedScript, cancellationToken);

                    foreach (var commandText in commandSegments)
                    {
                        cmd.CommandText = commandText;

                        await cmd.ExecuteNonQueryAsync(cancellationToken);
                    }

                    await migrationJournal.TrackExecutionAsync(script, cancellationToken);
                }
                else
                {
                    this.logger.LogInformation($"Skipping script '{script.Id}'");
                }
            }

            if (!executedAnyScript)
                this.logger.LogWarning("No migration scripts were executed!");
        }

        private async IAsyncEnumerable<Migration> GetScriptsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (var provider in this.scriptProviders)
            {
                var scripts = await provider.GetScriptsAsync(cancellationToken);

                foreach (var script in scripts)
                    yield return script;
            }
        }

        private async Task<IEnumerable<Migration>> HydrateMigrationScripts(CancellationToken cancellationToken)
        {
            var migrationList = new List<Migration>();
            await foreach (var migration in GetScriptsAsync(cancellationToken))
                migrationList.Add(migration);

            return migrationOrderer.OrderMigrations(migrationList);
        }

        private async Task<string> PreprocessScriptAsync(string script, CancellationToken cancellationToken)
        {
            foreach (var preprocessor in this.scriptPreprocessors)
            {
                script = await preprocessor.ProcessScriptAsync(script, cancellationToken);
            }

            return script;
        }
    }
}
