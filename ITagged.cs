using System.Collections.Generic;

namespace X.Google
{
    public interface ITagged
    {
        IEnumerable<string> Tags { get; }
        void SetTags(IEnumerable<string> tags);
    }
}
