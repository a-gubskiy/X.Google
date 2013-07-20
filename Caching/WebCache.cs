using System;
using System.Collections.Generic;

namespace X.Google.Caching
{
    public class WebCache : Cache
    {
        private readonly System.Web.Caching.Cache _cache;
        private readonly List<string> _cacheKeys = new List<string>();
        
        public WebCache(System.Web.Caching.Cache cache)
        {
            _cache = cache;
            CacheTimeout = 10 * 60;
        }

        public override void Clear()
        {
            foreach (var key in _cacheKeys)
            {
                try
                {
                    _cache.Remove(key);
                }
                catch { }
            }

            _cacheKeys.Clear();
        }

        public override void Insert(string key, object value)
        {
            _cache.Insert(key, value, null, DateTime.Now.AddSeconds(CacheTimeout), System.Web.Caching.Cache.NoSlidingExpiration);
        }

        public override void Remove(string key)
        {
            _cache.Remove(key);
        }

        public override Object this[string key]
        {
            get { return _cache[key]; }
            set { _cache[key] = value; }
        }

        public override void Dispose()
        {
        }
    }
}
