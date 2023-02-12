using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Security.Policy;

namespace WebTesting.Data
{
	public class RedditAPI
	{
		public RedditAPI()
		{
			Client = new HttpClient();
			Client.DefaultRequestHeaders.Add("user-Agent", "WebTesting/0.0.2");
			Client.BaseAddress = new Uri("https://oauth.reddit.com/");
		}
		public HttpClient Client { get; set; }

		public void SetAccessToken(string token)
		{
			Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
		}

		public async Task<(string id, string username)> GetIdAndNameAsync(string token) {
			Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
			var response = await Client.GetAsync("api/v1/me");
			string contents = await response.Content.ReadAsStringAsync();
			dynamic jsonData = JObject.Parse(contents);
			return (jsonData.id, jsonData.name);
		}
	}
}
