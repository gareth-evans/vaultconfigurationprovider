using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VaultConfiguration
{
    public class VaultClient
    {
        private readonly TokenAuthenticationProvider _tokenProvider;
        private readonly HttpClient _client;

        public VaultClient(
            Uri baseAddress,
            TokenAuthenticationProvider tokenProvider)
        {
            _client = new HttpClient {BaseAddress = baseAddress};
            _tokenProvider = tokenProvider;
        }

        private static string CoercePath(string path) => $"v1/{path.TrimStart('/')}";

        public async Task<JObject> GetList(string path)
        {
            var coercedPath = CoercePath(path);
            var request = new HttpRequestMessage(HttpMethod.Get, coercedPath);
            var token = _tokenProvider.GetToken();

            var response = await SendAsync(request, token).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var result = JsonConvert.DeserializeObject<JObject>(json);

            return result;
        }

        public async Task<VaultResponse> ReadSecretAsync(string path)
        {
            var coercedPath = CoercePath(path);
            var request = new HttpRequestMessage(HttpMethod.Get, coercedPath);
            var token = _tokenProvider.GetToken();

            var response = await SendAsync(request, token).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var result = JsonConvert.DeserializeObject<VaultResponse>(json);

            return result;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, string token)
        {
            request.Headers.Add("X-Vault-Token", token);

            var response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Someting went wrong: {error}");
            }
            return response;
        }

        public async Task WriteSecretAsync(string path, string value)
        {
            var coercedPath = CoercePath(path);
            var request = new HttpRequestMessage(HttpMethod.Post, coercedPath);
            var token = _tokenProvider.GetToken();

            request.Content = new StringContent(value, Encoding.UTF8, "application/json");

            await SendAsync(request, token);
        }
    }
}
