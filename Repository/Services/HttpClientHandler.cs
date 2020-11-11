using System.Net.Http;
using System.Threading.Tasks;

namespace Repository.Services
{
    public class HttpClientHandler : IHttpHandler
    {
        private readonly HttpClient _client = new HttpClient();

        public HttpResponseMessage Get(string url)
        {
            return GetAsync(url).Result;
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            return await _client.GetAsync(url);
        }

        public async Task<string> ReadAsStringAsync(HttpContent responseContent)
        {
            return await responseContent.ReadAsStringAsync();
        }
    }
}
