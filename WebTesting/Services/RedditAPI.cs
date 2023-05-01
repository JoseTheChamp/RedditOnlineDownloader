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
using static System.Net.Mime.MediaTypeNames;

namespace WebTesting.Services
{
	public class RedditAPI
	{
        private HttpClient client;

        public RedditAPI()
		{
			client = new HttpClient();
			client.DefaultRequestHeaders.Add("user-Agent", "WebTesting/0.0.3");
		}
        /// <summary>
        /// Gets profile info from /api/v1/me endpoint
        /// </summary>
        /// <param name="token">AccessToken of the user.</param>
        /// <returns></returns>
		public async Task<String> GetProfile(String token) {
			HttpRequestMessage request = new HttpRequestMessage();
			request.RequestUri = new Uri("https://oauth.reddit.com/api/v1/me");
			request.Method = HttpMethod.Get;
			request.Headers.Add("Authorization", $"Bearer {token}");
			var response = await client.SendAsync(request);
			return await response.Content.ReadAsStringAsync();
		}
        /// <summary>
        /// Get the last saved post from /user/[userName]/saved endpoint
        /// </summary>
        /// <param name="token">AccessToken of the user.</param>
        /// <param name="userName">Username of the user.</param>
        /// <returns>String that was returned by the endpoint.</returns>
		public async Task<String> GetLastSavedPost(String token,String userName) {
            //var response = await _reddit.Client.GetAsync("user/InnerPeace42/saved?limit=1");
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri($"https://oauth.reddit.com/user/{userName}/saved?limit=1");
            request.Method = HttpMethod.Get;
            request.Headers.Add("Authorization", $"Bearer {token}");
            var response = await client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
        /// <summary>
        /// Method sends x(max 10) requests to /user/[userName]/saved endpoint with different parameters until it gets all the posts saved on that account
        /// </summary>
        /// <param name="token"></param>
        /// <param name="userName"></param>
        /// <returns>List of formated posts.</returns>
        public async Task<List<Post>> GetAllSavedPosts(String token, String userName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{ \"data\": [");
            int returnedNumber = 100;
            int length = 0;
            int count = 0;
            string after = "";
            String list;
            String contents;
            HttpResponseMessage response;
            HttpRequestMessage request;
            dynamic jsonData;

            //Send requests until returned number of posts are not 100 or until all posts were returned. And make all these posts be in 1 JSON array
            while (returnedNumber == 100 && count < 10)
            {
                if (after != "")
                {
                    request = new HttpRequestMessage();
                    request.RequestUri = new Uri($"https://oauth.reddit.com/user/{userName}/saved?limit=100&after={after}");
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
                if (after != "")
                {
                    sb.Append("," + list); 
                }
                else
                {
                    sb.Append(list);
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
                try
                {
                    //Ignore deleted posts
                    if (jsonDataParse.data[i].data.removed_by_category != null)
                    {
                        continue;
                    }
                    //Parse comments
                    if (jsonDataParse.data[i].kind == "t1")
                    {
                        posts.Add(new Post(
                        jsonDataParse.data[i].data.id.ToString(),
                        jsonDataParse.data[i].data.body.ToString(),
                        "",
                        jsonDataParse.data[i].data.subreddit_name_prefixed.ToString(),//subreddit
                        jsonDataParse.data[i].data.author.ToString(),
                        "comment",
                        false,
                        jsonDataParse.data[i].data.permalink.ToString(),
                        Double.Parse(jsonDataParse.data[i].data.created_utc.ToString()),
                        Int32.Parse(jsonDataParse.data[i].data.ups.ToString()),
                        0,
                        new List<string>() { "https://www.reddit.com/" + jsonDataParse.data[i].data.permalink.ToString() }
                        ));
                        continue;
                    }

                    /*//debug
                    Debug.WriteLine(i);
                    dynamic post = jsonDataParse.data[i];
                    if (jsonDataParse.data[i].data.domain.ToString() == "gfycat.com")
                    {
                        int a = 5;
                    }*/

                    //Parsing of actual posts
                    List<string> urls = new List<string>();
                    string domain = jsonDataParse.data[i].data.domain.ToString();
                    //switch to do specificly needed operations per domain.
                    switch (domain)
                    {
                        case "reddit.com":
                            List<string> ids = new List<string>();
                            List<string> extensions = new List<string>();
                            List<string> orderIds = new List<string>();
                            int j = 0;
                            foreach (dynamic media in jsonDataParse.data[i].data.media_metadata)
                            {
                                JObject jo = (JObject)((JProperty)media).Value;
                                string m = (string)jo["m"];
                                extensions.Add(m.Substring(m.IndexOf('/') + 1));
                                ids.Add((string)jo["id"]);
                                j++;
                                //urls.Add("https://i.redd.it/" + id + "." + ext);
                            }
                            foreach (dynamic item in jsonDataParse.data[i].data.gallery_data.items)
                            {
                                orderIds.Add(item.media_id.ToString());
                            }
                            for (int k = 0; k < ids.Count; k++)
                            {
                                int index = ids.IndexOf(orderIds[k]);
                                urls.Add("https://i.redd.it/" + ids[index] + "." + extensions[index]);
                            }

                            break;
                        case "v.redd.it":
                            string crosspostUrl = "";
                            try
                            {
                                string crosspostParentId = jsonDataParse.data[i].data.crosspost_parent.ToString();
                                foreach (dynamic parent in jsonDataParse.data[i].data.crosspost_parent_list)
                                {
                                    if (parent.name == crosspostParentId)
                                    {
                                        crosspostUrl = parent.secure_media.reddit_video.fallback_url.ToString();
                                        break;
                                    }
                                }
                            }
                            catch (Exception) { }
                            if (crosspostUrl != "")
                            {
                                urls = new List<string> { crosspostUrl };
                            }
                            else
                            {
                                urls = new List<string> { jsonDataParse.data[i].data.secure_media.reddit_video.fallback_url.ToString() };
                            }
                            break;
                        case "gfycat.com":
                            urls = new List<string> { jsonDataParse.data[i].data.preview.reddit_video_preview.fallback_url.ToString() };
                            break;
                        default:
                            try {
                                urls = new List<string> { jsonDataParse.data[i].data.url.ToString() };
                            }
                            catch { }                      
                            break;
                    }
                    if (domain.StartsWith("self."))
                    {
                        domain = "text";
                    }
                    string hint = "";
                    try
                    {
                        hint = jsonDataParse.data[i].data.post_hint.ToString();
                    }
                    catch (Exception) { }
                    if (hint == "link")
                    {
                        if (domain != "v.redd.it")
                        {
                            domain = "link";
                        }
                    }
                    List<string> supportedDomains = new List<string>() {"i.redd.it","v.redd.it", "reddit.com", "i.imgur.com", "gfycat.com", "link", "text"};
                    if (!supportedDomains.Contains(domain))
                    {
                        domain = "not_supported";
                    }
                    //Creating new post and adding it to the final list
                    posts.Add(new Post(
                        jsonDataParse.data[i].data.id.ToString(),
                        jsonDataParse.data[i].data.title.ToString(),
                        jsonDataParse.data[i].data.selftext.ToString(),
                        jsonDataParse.data[i].data.subreddit_name_prefixed.ToString(),
                        jsonDataParse.data[i].data.author.ToString(),
                        domain,
                        jsonDataParse.data[i].data.over_18 == "True" ? true : false,
                        jsonDataParse.data[i].data.permalink.ToString(),
                        Double.Parse(jsonDataParse.data[i].data.created_utc.ToString()),
                        Int32.Parse(jsonDataParse.data[i].data.ups.ToString()),
                        Int32.Parse(jsonDataParse.data[i].data.num_comments.ToString()),
                        urls
                        ));
                }
                catch (Exception ex)
                {
                    //throw new Exception(posts[i].ToString());
                }
            }
            return posts;
        }
    }
}
