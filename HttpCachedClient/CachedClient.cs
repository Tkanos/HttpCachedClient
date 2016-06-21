using System;
using System.Runtime.Caching;


namespace HttpCachedClient
{
    public class CachedClient : IClient, ICacheCollection
    {
		public string ServiceUrl { get; set; }
        public string CollectionName { get; set; }

        private static MemoryCache _cache;
        private HttpBasicClient _client;

        public CachedClient(string baseUri, string collectionName)
        {
            this.ServiceUrl = baseUri;
            this.CollectionName = collectionName;

            if(_cache != null)
                _cache = new MemoryCache(collectionName);

            _client = new HttpBasicClient(baseUri);
        }

		public T Get<T>(string relativeUrl, DateTimeOffset? absoluteExpiration = null)
        {
            if(!absoluteExpiration.HasValue) // Do not use Caching
            {
                return GetResponse<T>(relativeUrl);
            }

            return AddOrGetExisting<T>(relativeUrl, () => GetResponse<T>(relativeUrl), absoluteExpiration.Value);
        }

        #region Protected Methods
        private T GetResponse<T>(string relativeUrl)
        {
            var task = _client.GetAsync<T>(relativeUrl);
            task.Wait();
            return task.Result;
        }

        protected T AddOrGetExisting<T>(string key, Func<T> valueFactory, DateTimeOffset absoluteExpiration)
        {
            var newValue = new Lazy<T>(valueFactory);
            var oldValue = _cache.AddOrGetExisting(key, newValue, new CacheItemPolicy
            {
                AbsoluteExpiration = absoluteExpiration
            }) as Lazy<T>;
            try
            {
                return (oldValue ?? newValue).Value;
            }
            catch
            {
                // Handle cached lazy exception by evicting from cache. Thanks to Denis Borovnev for pointing this out!
                _cache.Remove(key);
                throw;
            }
        }


        protected T AddOrGetExisting<T>(string key, Func<T> valueFactory, CacheItemPolicy cacheItempolicy)
        {
            var newValue = new Lazy<T>(valueFactory);
            var oldValue = _cache.AddOrGetExisting(key, newValue, cacheItempolicy) as Lazy<T>;
            try
            {
                return (oldValue ?? newValue).Value;
            }
            catch
            {
                // Handle cached lazy exception by evicting from cache. Thanks to Denis Borovnev for pointing this out!
                _cache.Remove(key);
                throw;
            }
        }
        #endregion



    }
}
