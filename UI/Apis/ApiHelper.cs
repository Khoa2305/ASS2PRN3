using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace UI.Apis
{
    public class ApiHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<HttpResponseMessage> RequestApi(
            string url,
            string method,
            object? body,
            HttpContext httpContext)
        {
            var request = new HttpRequestMessage(new HttpMethod(method), url);

            var accessToken = httpContext.Session.GetString("ACCESS_TOKEN");
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            }

            if (body != null && method != HttpMethod.Get.Method)
            {
                var json = JsonSerializer.Serialize(body);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return await _httpClient.SendAsync(request);
        }
    }
}
