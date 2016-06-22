using System;
using System.Runtime.Caching;


namespace HttpCachedClient
{
    public class CachedClient : ICacheCollection
    {
        public string CollectionName { get; set; }

        private MemoryCache _cache;
        private HttpBasicClient _client;

        public CachedClient(string baseUri, string collectionName = null)
        {
            this.CollectionName = string.IsNullOrEmpty(collectionName) ? baseUri : collectionName;

            _cache = new MemoryCache(this.CollectionName);

            _client = new HttpBasicClient(baseUri);
        }

        /// <summary>
        /// Gets the specified relative URL.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="relativeUrl">The relative URL.</param>
        /// <param name="absoluteExpiration">The absolute expiration.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public T Get<T>(string relativeUrl, DateTimeOffset? absoluteExpiration = null, string key = null)
        {
            return Function(key ?? relativeUrl, () => GetResponse<T>(relativeUrl), absoluteExpiration); 
        }

        /// <summary>
        /// Gets the specified relative URL.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="relativeUrl">The relative URL.</param>
        /// <param name="policy">The policy.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public T Get<T>(string relativeUrl, CacheItemPolicy policy, string key = null)
        {
            return Function(key ?? relativeUrl, () => GetResponse<T>(relativeUrl), policy);
        }

        /// <summary>
        /// Posts the specified relative URL.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="relativeUrl">The relative URL.</param>
        /// <param name="data">The data.</param>
        /// <param name="absoluteExpiration">The absolute expiration.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public T Post<T>(string relativeUrl, string data, DateTimeOffset? absoluteExpiration = null, string key = null)
        {
            return Function(key ?? relativeUrl, () => PostResponse<T>(relativeUrl, data), absoluteExpiration);
        }

        /// <summary>
        /// Posts the specified relative URL.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="relativeUrl">The relative URL.</param>
        /// <param name="data">The data.</param>
        /// <param name="policy">The policy.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public T Post<T>(string relativeUrl, string data, CacheItemPolicy policy = null, string key = null)
        {
            return Function(key ?? relativeUrl, () => PostResponse<T>(relativeUrl, data), policy);
        }

        /// <summary>
        /// Functions the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="method">The method.</param>
        /// <param name="absoluteExpiration">The absolute expiration.</param>
        /// <returns></returns>
        private T Function<T> (string key, Func<T> method, DateTimeOffset? absoluteExpiration = null )
        {
            if (!absoluteExpiration.HasValue) // Do not use Caching
            {
                return method.Invoke();
            }

            return AddOrGetExisting<T>(key,  method, absoluteExpiration.Value);
        }

        private T Function<T>(string key, Func<T> method, CacheItemPolicy policy = null )
        {

            return AddOrGetExisting<T>(key, method, policy);
        }

        #region Protected Methods
        /// <summary>
        /// Gets the response.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="relativeUrl">The relative URL.</param>
        /// <returns></returns>
        private T GetResponse<T>(string relativeUrl)
        {
            var task = _client.GetAsync<T>(relativeUrl);
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// Posts the response.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="relativeUrl">The relative URL.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private T PostResponse<T>(string relativeUrl, string data)
        {
            var task = _client.PostAsync<T>(relativeUrl, data);
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// Adds the or get existing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <param name="absoluteExpiration">The absolute expiration.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Adds the or get existing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <param name="cacheItempolicy">The cache itempolicy.</param>
        /// <returns></returns>
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
