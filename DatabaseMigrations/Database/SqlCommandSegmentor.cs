using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseMigrations.Database
{
    public interface ISqlCommandSegmentor
    {
        Task<IEnumerable<string>> SegementScriptAsync(string script, CancellationToken cancellationToken);
    }

    public class RegexSqlCommandSegmentor : ISqlCommandSegmentor
    {
        public string CommandSeperatorRegex { get; set; } = @"\sgo\s";

        public Task<IEnumerable<string>> SegementScriptAsync(string script, CancellationToken cancellationToken)
        {
            var parts = Regex.Split(script, CommandSeperatorRegex, RegexOptions.IgnoreCase);

            return Task.FromResult<IEnumerable<string>>(parts);
        }
    }
}
