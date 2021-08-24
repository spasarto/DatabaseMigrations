using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigrations.Tests
{
    static class Connections
    {
        public static DbConnection SqliteDbConnFactory(IServiceProvider sp) => new SqliteConnection("Data Source=:memory:");
        public static DbConnection SqlServerDbConnFactory(IServiceProvider sp) => new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

    }
}
