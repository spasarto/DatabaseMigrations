using System;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseMigrations.ScriptPreprocessors
{
    public class VariableSubstitutionPreprocessor : IScriptPreprocessor
    {
        private readonly string variableName;
        private readonly Func<string> value;

        public VariableSubstitutionPreprocessor(string variableName, Func<string> value)
        {
            this.variableName = variableName;
            this.value = value;
        }

        public Task<string> ProcessScriptAsync(string script, CancellationToken cancellationToken)
        {
            script = script.Replace(variableName, value());

            return Task.FromResult(script);
        }
    }
}
