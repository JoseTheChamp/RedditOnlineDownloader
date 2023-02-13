using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Net;
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
		}
		public HttpClient Client { get; set; }
		public async Task<(string id, string username)> GetIdAndNameAsync(string token) {
			HttpRequestMessage request = new HttpRequestMessage();
			request.RequestUri = new Uri("https://oauth.reddit.com/api/v1/me");
			request.Method = HttpMethod.Get;
			request.Headers.Add("Authorization", $"Bearer {token}");
			var response = await Client.SendAsync(request);
			string contents = await response.Content.ReadAsStringAsync();
			dynamic jsonData = JObject.Parse(contents);
			Client.DefaultRequestHeaders.Authorization = null;
			return (jsonData.id, jsonData.name);
			//TODO ma vracet pouze responce/content

			/*
			Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
			var response = await Client.GetAsync("api/v1/me");
			string contents = await response.Content.ReadAsStringAsync();
			dynamic jsonData = JObject.Parse(contents);
			Client.DefaultRequestHeaders.Authorization = null;
			return (jsonData.id, jsonData.name);*/
		}
	}
}
