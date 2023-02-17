using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System.Net.Http.Headers;
using System.Security.Policy;
using WebTesting.Entities;
using WebTesting.Entities.Enums;
using WebTesting.Services;
using WebTesting.Utils;

namespace WebTesting.Pages
{
	public class LastSavedPostModel : PageModel
	{
		public String Title { get; set; }
		public String Type { get; set; }
		public PostsResult SessionResult { get; set; }
		public List<Post> Posts{ get; set; }

		private readonly RedditAPI _reddit;

		public LastSavedPostModel(RedditAPI reddit)
		{
			_reddit = reddit;
			Posts= new List<Post>();
		}
		public async Task OnGetAsync()
		{
			try
			{
				if (HttpContext.Session.GetString("Posts") != null)
				{
					Posts = HttpContext.Session.GetObject<List<Post>>("Posts");
				}
				else {
					List<Post> posts = await _reddit.GetAllSavedPosts(HttpContext.Session.GetString("AccessToken"), HttpContext.Session.GetString("UserName"));
					HttpContext.Session.SetObject("Posts", posts);
					Posts = HttpContext.Session.GetObject<List<Post>>("Posts");
				}
				string json = await _reddit.GetLastSavedPost(HttpContext.Session.GetString("AccessToken"), HttpContext.Session.GetString("UserName"));
				var result = ResponseConvertor.PostToTitleType(json);
				Title = result.title;
				Type = result.type;
				
			}
			catch (Exception)
			{

				throw;
			}
			
			/*SessionResult = HttpContext.Session.GetObject<PostsResult>("PostsStatus");
			if (SessionResult == PostsResult.Success)
			{
				Posts = HttpContext.Session.GetObject<List<Post>>("Posts");
			}*/
		}
	}
}
