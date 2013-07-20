using System;


namespace X.Google.Caching
{
    public class FakeCache : Cache
    {
        public FakeCache()
        {
        }

        public override void Insert(String key, object value)
        {
        }

        public override void Remove(String key)
        {
        }

        public override void Clear()
        {
        }

        public override object this[String key]
        {
            get { return null; }
            set { }
            
        }

        public override void Dispose()
        {
        }
    }
}
