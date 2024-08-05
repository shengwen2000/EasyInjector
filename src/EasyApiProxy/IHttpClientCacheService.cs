using System;
using System.Collections.Generic;
using System.Net.Http;

namespace EasyApiProxys
{
    /// <summary>
    /// HttpClient 快取服務
    /// </summary>
    public interface IHttpClientCacheService
    {
        /// <summary>
        /// 取得 HttpClient，或建立之
        /// </summary>        
        HttpClient GetOrCreateHttpClient(string key, Func<HttpClient> createFunc);
     
    }

    public class HttpClientCacheService : IHttpClientCacheService
    {
        internal static IHttpClientCacheService Shared { get; set; }

        Dictionary<string, HttpClient> _caches = new Dictionary<string, HttpClient>();

        static HttpClientCacheService()
        {
            Shared = new HttpClientCacheService();
        }

        public HttpClient GetOrCreateHttpClient(string cacheName, Func<HttpClient> createHttpClient)
        {
            lock (_caches)
            {
                HttpClient c1 = null;
                if (_caches.TryGetValue(cacheName, out c1))
                    return c1;
                else
                {
                    c1 = createHttpClient();
                    _caches.Add(cacheName, c1);
                    return c1;
                }
            }
        }
    }
}
