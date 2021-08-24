using System;
using System.Data.Common;

namespace DatabaseMigrations.Database
{
    public class UnownedConnectionProviderIConnectionProvider : IConnectionProvider
    {
        private readonly Func<DbConnection> connFactory;

        public UnownedConnectionProviderIConnectionProvider(Func<DbConnection> connFactory)
        {
            this.connFactory = connFactory;
        }

        public void Dispose() { }

        public DbConnection Get() => connFactory();
    }
}
