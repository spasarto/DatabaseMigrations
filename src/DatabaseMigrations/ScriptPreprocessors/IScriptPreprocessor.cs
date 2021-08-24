using System.Threading;
using System.Threading.Tasks;

namespace DatabaseMigrations.ScriptPreprocessors
{
    public interface IScriptPreprocessor
    {
        Task<string> ProcessScriptAsync(string script, CancellationToken cancellationToken);
    }
}
