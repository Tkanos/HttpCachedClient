using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace HttpCachedClient
{
    public class HttpBasicClient : IClient
    {
        public string ServiceUrl { get; set; }

        public HttpBasicClient(string baseUri)
        {
            this.ServiceUrl = baseUri;
        }

        public async Task<T> GetAsync<T>(string relativeUrl)
        {
            T result = default(T);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.ServiceUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(relativeUrl).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsAsync<T>();
                }
            }

            return result;
        }

    }
}
