
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text;
using System.Web;

namespace Goro.Check.Cache
{
    public class MemoryCacheService : ICacheService
    {
        protected MemoryCache _cache;

        public MemoryCacheService(MemoryCache cache)
        {
            _cache = cache;
        }

        public T Get<T>(string key) where T : class, new()
        {
            return _cache.Get(key) as T;
        }

        public string Get(string key)
        {
            var res = _cache.Get(key);
            if (res != null)
                return res.ToString();

            return "";
        }

        public void Remove(string key)
        {
             _cache.Remove(key);
        }

        public void RemoveAll()
        {
            throw new NotImplementedException();
        }

        public void Set(string key, object value, TimeSpan expirTimeSpan)
        {
            _cache.Set(key, value, DateTime.Now.Add(expirTimeSpan));
        }
    }
}
