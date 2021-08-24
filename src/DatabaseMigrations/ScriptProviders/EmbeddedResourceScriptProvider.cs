using DatabaseMigrations.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseMigrations.ScriptProviders
{
    public class EmbeddedResourceScriptProvider : IScriptProvider
    {
        private readonly Assembly assembly;
        private readonly Func<string, bool> resourceNamesFilter;

        public EmbeddedResourceScriptProvider(Assembly assembly, Func<string, bool> resourceNamesFilter)
        {
            this.assembly = assembly;
            this.resourceNamesFilter = resourceNamesFilter;
        }

        public Task<IEnumerable<Migration>> GetScriptsAsync(CancellationToken cancellationToken)
        {
            var names = assembly.GetManifestResourceNames()
                                .Where(n => resourceNamesFilter(n))
                                .Select(n => new Migration(n, GetResourceContent(n)));

            return Task.FromResult(names);
        }

        private string GetResourceContent(string n)
        {
            using var stream = assembly.GetManifestResourceStream(n);
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}
