using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseMigrations.Database
{
    public class UnownedConnectionProviderIConnectionProvider : IConnectionProvider
    {
        private readonly Func<DbConnection> connFactory;

        public UnownedConnectionProviderIConnectionProvider(Func<DbConnection> connFactory)
        {
            this.connFactory = connFactory;
        }

        public ValueTask DisposeAsync() => new ValueTask();

        public Task<DbConnection> GetAsync(CancellationToken cancellationToken) => Task.FromResult(connFactory());
    }
}
