using System;

namespace X.Google.Caching
{
    [Serializable]
    public class CacheItem
    {
        public CacheItem()
        {
            Key = String.Empty;
            Value = null;
            TimeStamp = DateTime.Now;
        }

        public CacheItem(string key, object value)
        {
            Key = key;
            Value = value;
            TimeStamp = DateTime.Now;
        }

        public DateTime TimeStamp { get; set; }
        public String Key { get; set; }
        public Object Value { get; set; }
    }
}
