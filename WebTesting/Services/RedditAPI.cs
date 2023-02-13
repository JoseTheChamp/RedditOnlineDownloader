using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Policy;

namespace WebTesting.Services
{
	public class RedditAPI
	{
		public RedditAPI()
		{
			client = new HttpClient();
			client.DefaultRequestHeaders.Add("user-Agent", "WebTesting/0.0.2");
		}
		private HttpClient client;
		public async Task<String> GetProfile(String token) {
			HttpRequestMessage request = new HttpRequestMessage();
			request.RequestUri = new Uri("https://oauth.reddit.com/api/v1/me");
			request.Method = HttpMethod.Get;
			request.Headers.Add("Authorization", $"Bearer {token}");
			var response = await client.SendAsync(request);
			return await response.Content.ReadAsStringAsync();
		}
		public async Task<String> GetLastSavedPost(String token,String userName) {
            //var response = await _reddit.Client.GetAsync("user/InnerPeace42/saved?limit=1");
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri($"https://oauth.reddit.com/user/{userName}/saved?limit=1");
            request.Method = HttpMethod.Get;
            request.Headers.Add("Authorization", $"Bearer {token}");
            var response = await client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
	}
}
