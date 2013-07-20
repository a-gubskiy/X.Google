using System;

namespace X.Google.Caching
{
    [Serializable]
    public abstract class Cache : IDisposable
    {
        /// <summary>
        /// Cache timeout in seconds
        /// </summary>
        public int CacheTimeout { get; set; }

        public Cache()
        {
            CacheTimeout = 10 * 60;
        }

        public abstract void Insert(string key, object value);
        public abstract void Remove(string key);
        public abstract void Clear();

        public abstract Object this[string key] { get; set; }

        public abstract void Dispose();
    }
}
