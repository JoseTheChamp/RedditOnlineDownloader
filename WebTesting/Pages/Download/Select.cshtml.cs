using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTesting.Entities;
using WebTesting.Services;
using WebTesting.Utils;

namespace WebTesting.Pages.Download
{
    public class SelectModel : PageModel
    {
        public List<Post> AllPosts { get; set; }
        public List<Post> Posts { get; set; }
        private readonly RedditAPI _reddit;
        public SelectModel(RedditAPI reddit)
        {
            _reddit = reddit;
        }
        public async Task OnGetAsync()
        {
            if (HttpContext.Session.GetString("Posts") != null)
            {
                AllPosts = HttpContext.Session.GetObject<List<Post>>("Posts");
            }
            else
            {
                List<Post> posts = await _reddit.GetAllSavedPosts(HttpContext.Session.GetString("AccessToken"), HttpContext.Session.GetString("UserName"));
                HttpContext.Session.SetObject("Posts", posts);
                AllPosts = HttpContext.Session.GetObject<List<Post>>("Posts");
            }
            Posts = AllPosts;
        }
    }
}
