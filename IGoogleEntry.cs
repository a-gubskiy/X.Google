using System;
using System.Collections.Generic;

namespace X.Google
{
    public interface IGoogleEntry
    {
        string Id { get; }
        string Title { get; }
        string Description { get; }
        string Url { get; }
        DateTime TimeStamp { get; }
        IEnumerable<string> Tags { get; }
    }
}
