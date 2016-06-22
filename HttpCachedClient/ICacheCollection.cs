
using System;

namespace HttpCachedClient
{
    public interface ICacheCollection
    {
        string CollectionName { get; set; }

        T Get<T>(string relativeUrl, DateTimeOffset? absoluteExpiration = null, string key = null);

        T Post<T>(string relativeUrl, string data, DateTimeOffset? absoluteExpiration = null, string key = null);
    }
}
