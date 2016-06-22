
using System.Threading.Tasks;

namespace HttpCachedClient
{
    public interface IClient
    {
        string ServiceUrl { get; set; }

        Task<T> GetAsync<T>(string relativeUrl);

        Task<T> PostAsync<T>(string relativeUrl, string data);
    }
}
