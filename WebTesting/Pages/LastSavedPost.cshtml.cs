using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System.Net.Http.Headers;
using System.Security.Policy;
using WebTesting.Services;

namespace WebTesting.Pages
{
	public class LastSavedPostModel : PageModel
	{
		public String Title { get; set; }
		public String Type { get; set; }

		private readonly RedditAPI _reddit;

		public LastSavedPostModel(RedditAPI reddit)
		{
			_reddit = reddit;
		}
		public async Task OnGetAsync()
		{
			string json = await _reddit.GetLastSavedPost(HttpContext.Session.GetString("AccessToken"), HttpContext.Session.GetString("UserName"));
			var result = ResponseConvertor.PostToTitleType(json);
			Title = result.title; 
			Type = result.type;
		}
	}
}
