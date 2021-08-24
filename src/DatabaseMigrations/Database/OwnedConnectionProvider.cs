using System;
using System.Data;
using System.Data.Common;

namespace DatabaseMigrations.Database
{
    public class OwnedConnectionProvider : IConnectionProvider
    {
        private readonly Lazy<DbConnection> getConnection;

        public OwnedConnectionProvider(Func<DbConnection> dbFactory)
        {
            this.getConnection = new Lazy<DbConnection>(() =>
            {
                var conn = dbFactory() ?? throw new ArgumentNullException("Database connection must not be null");

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                return conn;
            });
        }

        public void Dispose()
        {
            if (getConnection.IsValueCreated)
                getConnection.Value.Dispose();
        }

        public DbConnection Get() => getConnection.Value;
    }
}
