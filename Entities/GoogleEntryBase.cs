using System;
using System.Collections.Generic;

namespace X.Google
{
    [Serializable]
    public abstract class GoogleEntryBase : IGoogleEntry
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public DateTime TimeStamp { get; set; }

        public abstract IEnumerable<string> Tags { get; }
                
        public GoogleEntryBase()
        {
        }
    }
}
