using DatabaseMigrations.ScriptPreprocessors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JournalWithScope
{
    class ScopeVariablePreprocessor : IScriptPreprocessor
    {
        private readonly ICustomScopeProvider customScopeProvider;

        public ScopeVariablePreprocessor(ICustomScopeProvider customScopeProvider)
        {
            this.customScopeProvider = customScopeProvider;
        }

        public Task<string> ProcessScriptAsync(string script, CancellationToken cancellationToken)
            => Task.FromResult(script.Replace("$scope$", customScopeProvider.Scope));
    }
}
