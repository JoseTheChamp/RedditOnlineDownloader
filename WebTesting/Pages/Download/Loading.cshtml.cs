using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTesting.Entities;
using WebTesting.Models;
using WebTesting.Services;
using WebTesting.Utils;

namespace WebTesting.Pages.Download
{
    public class LoadingModel : PageModel
    {
        private readonly RedditAPI _reddit;

        [ActivatorUtilitiesConstructor]
        public LoadingModel(RedditAPI reddit)
        {
            _reddit = reddit;
        }

        public void OnGet() { 
        
        }
        public async Task OnGetPostsAsync()
        {
            if (HttpContext.Session.GetString("AllPosts") == null)
            {
                //Fetching all the posts from reddit - long operation
                List<Post> posts = await _reddit.GetAllSavedPosts(HttpContext.Session.GetString("AccessToken"), HttpContext.Session.GetString("UserName"));
                HttpContext.Session.SetObject("AllPosts", posts);
            }
        }
    }
}
