using DatabaseMigrations.Database;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseMigrations.ScriptProviders
{
    public interface IScriptProvider
    {
        Task<IEnumerable<Migration>> GetScriptsAsync(CancellationToken cancellationToken);
    }
}
