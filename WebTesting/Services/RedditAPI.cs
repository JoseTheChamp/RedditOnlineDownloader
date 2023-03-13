using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using WebTesting.Entities;

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

        //public async Task<Stream> GetImage

        public async Task<List<Post>> GetAllSavedPosts(String token, String userName)
        {
            //Setting up request messsage
            

            //Make one Json object with all posts inside.
            StringBuilder sb = new StringBuilder();
            sb.Append("{ \"data\": [");
            int returnedNumber = 100;
            int length = 0;
            bool first = true;
            int count = 0;
            string after = "";
            String list;
            String contents;
            HttpResponseMessage response;
            HttpRequestMessage request;
            dynamic jsonData;
            while (returnedNumber == 100 && count < 10)
            {
                if (after != "")
                {
                    request = new HttpRequestMessage();
                    request.RequestUri = new Uri($"https://oauth.reddit.com/user/{userName}/saved?limit=100&after=" + after);
                    request.Method = HttpMethod.Get;
                    request.Headers.Add("Authorization", $"Bearer {token}");
                    
                }
                else {
                    request = new HttpRequestMessage();
                    request.RequestUri = new Uri($"https://oauth.reddit.com/user/{userName}/saved?limit=100");
                    request.Method = HttpMethod.Get;
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }
                response = await client.SendAsync(request);
                contents = await response.Content.ReadAsStringAsync();
                jsonData = JObject.Parse(contents);
                list = "" + jsonData.data.children;
                list = list.Substring(1, list.Length - 2);
                if (first)
                {
                    first = false;
                    sb.Append(list);
                }
                else
                {
                    sb.Append("," + list);
                }
                returnedNumber = jsonData.data.dist;
                after = jsonData.data.after;
                length = length + returnedNumber;
                count++;
            }
            sb.Append("], \"NumberOfPosts\": " + length + "}");

            //convert json object to list of posts
            dynamic jsonDataParse = JObject.Parse(sb.ToString());
            List<Post> posts = new List<Post>();
            int numberOfPosts = Int32.Parse(jsonDataParse.NumberOfPosts.ToString());
            for (int i = 0; i < numberOfPosts; i++)
            {
                if (jsonDataParse.data[i].data.removed_by_category != null)
                {
                    continue; //TODO Maybe some reporting how many were removed
                }
                /*Debug.WriteLine(i);
                if (i == 84) {
                    int abf = 5;
                }*/
                if (jsonDataParse.data[i].kind == "t1") {
                    continue; //TODO allow for saving comments
                }
                List<string> urls = new List<string>();
                //dynamic post = jsonDataParse.data[i];
                switch (jsonDataParse.data[i].data.domain.ToString())
                {
                    case "reddit.com":
                        foreach (dynamic media in jsonDataParse.data[i].data.media_metadata)
                        {
                            JObject jo = (JObject)((JProperty)media).Value;
                            string m = (string)jo["m"];
                            string ext = m.Substring(m.IndexOf('/') + 1);
                            string id = (string)jo["id"];
                            urls.Add("https://i.redd.it/" + id + "." + ext);
                        }
                        break;
                    case "v.redd.it":
                        urls = new List<string> { jsonDataParse.data[i].data.secure_media.reddit_video.fallback_url.ToString() };
                        break;
                    default:
                        urls = new List<string> { jsonDataParse.data[i].data.url.ToString() };
                        break;
                }
                posts.Add(new Post(
                    jsonDataParse.data[i].data.id.ToString(),
                    jsonDataParse.data[i].data.title.ToString(),
                    jsonDataParse.data[i].data.selftext.ToString(),
                    jsonDataParse.data[i].data.subreddit_name_prefixed.ToString(),//subreddit
                    jsonDataParse.data[i].data.author.ToString(),
                    jsonDataParse.data[i].data.domain.ToString(),
                    jsonDataParse.data[i].data.over_18 == "True" ? true : false,
                    jsonDataParse.data[i].data.permalink.ToString(),
                    Double.Parse(jsonDataParse.data[i].data.created_utc.ToString()),
                    Int32.Parse(jsonDataParse.data[i].data.ups.ToString()),
                    Int32.Parse(jsonDataParse.data[i].data.num_comments.ToString()),
                    //links
                    urls
                    ));
            }
            int a = 5;
            return posts;
        }
    }
}
