using Microsoft.Extensions.Options;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseMigrations.Database
{
    public class OwnedConnectionProvider : IConnectionProvider
    {
        private bool init;
        private readonly Func<CancellationToken, Task<DbConnection>> getConnection;

        public OwnedConnectionProvider(Func<DbConnection> dbFactory)
        {
            DbConnection? conn = null;
            this.getConnection = async ct =>
            {
                if (!init)
                {
                    conn = dbFactory() ?? throw new ArgumentNullException("Database connection must not be null");

                    if (conn.State != ConnectionState.Open)
                        await conn.OpenAsync(ct);

                    init = true;
                }

                return conn!;
            };
        }

        public async ValueTask DisposeAsync()
        {
            if (init)
            {
                var conn = await this.getConnection(CancellationToken.None);
                await conn.DisposeAsync();
            }
        }

        public Task<DbConnection> GetAsync(CancellationToken cancellationToken) => getConnection(cancellationToken);
    }
}
