using System.Net.Http;
using System.Threading.Tasks;

namespace Repository.Services
{
    public interface IHttpHandler
    {
        HttpResponseMessage Get(string url);
        Task<HttpResponseMessage> GetAsync(string url);
        Task<string> ReadAsStringAsync(HttpContent responseContent);
    }
}