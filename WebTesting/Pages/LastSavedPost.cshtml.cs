using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System.Net.Http.Headers;
using System.Security.Policy;
using WebTesting.Data;

namespace WebTesting.Pages
{
	public class LastSavedPostModel : PageModel
	{
		public String Title { get; set; }
		public String Type { get; set; }

		private RedditAPI _reddit;
		public LastSavedPostModel(RedditAPI reddit)
		{
			_reddit = reddit;
		}
		public async Task OnGetAsync(RedditAPI reddit)
		{
			var response = await _reddit.Client.GetAsync("user/InnerPeace42/saved?limit=1");
			String post = await response.Content.ReadAsStringAsync();
			dynamic jsonData = JObject.Parse(post);
			string domain = jsonData.data.children[0].data.domain;
			switch (domain)
			{
				case "i.redd.it":
					Type = "Reddit image.";
					break;
				case "v.redd.it":
					Type = "Reddit video.";
					break;
				case "i.imgur.com":
					Type = "Imgur image.";
					break;
				default:
					Type = "Reddit text post.";
					break;
			}
			Title = jsonData.data.children[0].data.title;
		}
	}
}
