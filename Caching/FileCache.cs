using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace X.Google.Caching
{
    [Serializable]
    public class FileCache : X.Google.Caching.Cache
    {
        public FileCache()
        {
        }

        private List<CacheItem> _dictionary;

        private List<CacheItem> Dictionary
        {
            get
            {
                if (_dictionary == null)
                {
                    _dictionary = LoadCache();
                }

                var timeStamp = (from x in _dictionary
                                 orderby x.TimeStamp descending
                                 select x.TimeStamp).FirstOrDefault();

                if (timeStamp.AddMinutes(5) < DateTime.Now)
                {
                    _dictionary = new List<CacheItem>();
                }

                return _dictionary;
            }
        }

        private List<CacheItem> LoadCache()
        {
            Stream stream = null;
            List<CacheItem> dictionary = new List<CacheItem>();

            try
            {
                stream = File.Open(CacheFilePath, FileMode.Open);
                var formatter = new BinaryFormatter();
                dictionary = (List<CacheItem>)formatter.Deserialize(stream);
                stream.Close();
            }
            catch
            {

            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            return dictionary;
        }

        private void SaveCache()
        {
            var stream = File.Open(CacheFilePath, FileMode.Create);
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, _dictionary);
            stream.Close();
        }

        protected string CacheFilePath { get; set; }

        public FileCache(string path)
        {
            CacheFilePath = path;
        }

        public override void Insert(string key, object value)
        {
            Remove(key);
            _dictionary.Add(new CacheItem(key, value));

            SaveCache();
        }

        public override void Remove(string key)
        {
            var item = Dictionary.Where(x => x.Key == key).SingleOrDefault();

            if (item != null)
            {
                Dictionary.Remove(item);
            }
        }

        public override void Clear()
        {
            Dictionary.Clear();
            SaveCache();
        }

        public override object this[string key]
        {
            get
            {
                var cacheItem = Dictionary.Where(x => x.Key == key).SingleOrDefault();

                if (cacheItem != null)
                {
                    return cacheItem.Value;
                }

                return null;
            }
            set { Insert(key, value); }
        }

        public override void Dispose()
        {
            SaveCache();
        }
    }
}
