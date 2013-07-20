using System;
using System.Collections.Generic;

namespace X.Google
{
    [Serializable]
    public class Publication : GoogleEntryBase, ITagged
    {
        private readonly List<string> _tags = new List<string>();

        public string Content { get; set; }
        public override IEnumerable<string> Tags { get { return _tags; } }
        public string BlogId { get; set; }

        public void SetTags(IEnumerable<string> tags)
        {
            _tags.Clear();
            _tags.AddRange(tags);
        }
    }
}
