using System;

namespace JournalWithScope
{
    public interface ICustomScopeProvider
    {
        string Scope { get; set; }
    }

    public class CustomScopeProvider : ICustomScopeProvider
    {
        public string Scope { get; set; }
    }
}
