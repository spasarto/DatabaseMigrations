using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseMigrations.Database
{
    public interface IConnectionProvider : IDisposable
    {
        DbConnection Get();
    }
}
