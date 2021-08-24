using DatabaseMigrations;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JournalWithScope
{
    class RunMigrationsService : BackgroundService
    {
        private readonly IDatabaseMigrator databaseMigrator;
        private readonly ICustomScopeProvider customScopeProvider;

        public RunMigrationsService(IDatabaseMigrator databaseMigrator, ICustomScopeProvider customScopeProvider)
        {
            this.databaseMigrator = databaseMigrator;
            this.customScopeProvider = customScopeProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.Write("Scope: ");
                string scope = Console.ReadLine();

                if (!string.IsNullOrEmpty(scope))
                {
                    customScopeProvider.Scope = scope;

                    await this.databaseMigrator.ApplyMigrationsAsync(stoppingToken);
                }
            }
        }
    }
}
