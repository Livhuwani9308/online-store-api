using System.Text;
using System.Text.Json;

namespace online_store_api.Services
{
    public class SirvAuthService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private string _accessToken;
        private DateTime _expiryTime;

        public SirvAuthService(IConfiguration config)
        {
            _config = config;
            _httpClient = new HttpClient();
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && _expiryTime > DateTime.UtcNow)
                return _accessToken;

            var body = new
            {
                clientId = _config["SirvSettings:ClientId"],
                clientSecret = _config["SirvSettings:ClientSecret"]
            };

            var response = await _httpClient.PostAsync(
                $"{_config["SirvSettings:BaseUrl"]}/token",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            _accessToken = doc.RootElement.GetProperty("token").GetString();
            var expiresIn = doc.RootElement.GetProperty("expiresIn").GetInt32();

            _expiryTime = DateTime.UtcNow.AddSeconds(expiresIn - 60); // renew a minute early
            return _accessToken;
        }
    }
}
