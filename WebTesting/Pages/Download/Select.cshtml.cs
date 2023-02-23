using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using WebTesting.Entities;
using WebTesting.Entities.Enums;
using WebTesting.Services;
using WebTesting.Utils;

namespace WebTesting.Pages.Download
{
    public class SelectModel : PageModel
    {
        public List<Post> AllPosts { get; set; }
        public List<Post> Posts { get; set; }
        public SelectShowType ShowType { get; set; }
        public SelectNsfw Nsfw { get; set; }
        private readonly RedditAPI _reddit;
        public SelectModel(RedditAPI reddit)
        {
            _reddit = reddit;
        }
        public async Task OnGetAsync()
        {
            if (HttpContext.Session.GetString("AllPosts") != null)
            {
                Posts = HttpContext.Session.GetObject<List<Post>>("Posts");
                AllPosts = HttpContext.Session.GetObject<List<Post>>("AllPosts");
            }
            else
            {
                List<Post> posts = await _reddit.GetAllSavedPosts(HttpContext.Session.GetString("AccessToken"), HttpContext.Session.GetString("UserName"));
                HttpContext.Session.SetObject("AllPosts", posts);
                HttpContext.Session.SetObject("Posts", posts);
                AllPosts = posts;
                Posts = posts;
            }
        }
        public void OnGetChangeShowType() {
            int a = 5;
        }
        public IActionResult OnPostChangeShowType(string showType)
        {
            Posts = HttpContext.Session.GetObject<List<Post>>("Posts");
            AllPosts = HttpContext.Session.GetObject<List<Post>>("AllPosts");

            //TODO load all filters of Select

            switch (showType) //TODO not ideal solution change in html will brake this.
            {
                case "1":
                    ShowType = SelectShowType.POSTS;
                    break;
                case "2":
                    ShowType = SelectShowType.SUBREDDIT_EXPANDED;
                    break;
                case "3":
                    ShowType = SelectShowType.SUBREDDITS_REDUCED;
                    break;
                default:
                    throw new Exception("In select there is showtype that does not exist.");
            }
            HttpContext.Session.SetObject("ShowType", ShowType);
            return Page();
        }
        public IActionResult OnPostSelect()
        {
            AllPosts = HttpContext.Session.GetObject<List<Post>>("AllPosts");
            Posts = AllPosts;
            ShowType = HttpContext.Session.GetObject<SelectShowType>("ShowType");

            string nsfw = Request.Form["nsfw"];
            switch (nsfw) //TODO not ideal solution change in html will brake this.
            {
                case "sfw":
                    Posts = Posts.Where(e => !e.Over18).ToList();
                    Nsfw = SelectNsfw.SFW;
                    break;
                case "nsfw":
                    Posts = Posts.Where(e => e.Over18).ToList();
                    Nsfw = SelectNsfw.NSFW;
                    break;
                case "both":
                    Nsfw = SelectNsfw.BOTH;
                    break;
                default:
                    throw new Exception("In select there is nsfw that does not exist.");
            }
            HttpContext.Session.SetObject("Posts",Posts);
            return Page();
        }
        public IActionResult OnPost()
        {
            Posts = HttpContext.Session.GetObject<List<Post>>("Posts");
            AllPosts = HttpContext.Session.GetObject<List<Post>>("AllPosts");
            int a = 5;
            return Page();
        }
    }
}
