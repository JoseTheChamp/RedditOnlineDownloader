using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NuGet.Packaging.Signing;
using NuGet.Protocol;
using System.Runtime.Intrinsics.Arm;
using WebTesting.Entities;
using WebTesting.Entities.Enums;
using WebTesting.Models;
using WebTesting.Services;
using WebTesting.Utils;

namespace WebTesting.Pages.Download
{
    public class SelectModel : PageModel
    {
        public List<Post> AllPosts { get; set; }
        public List<Post> Posts { get; set; }
        public string PostsJson { get; set; }
        public List<Post> SelectedPosts { get; set; }
        public List<string> DownloadedIds { get; set; }
        public SelectShowType ShowType { get; set; }
        public SelectNsfw Nsfw { get; set; }
        public bool ShowDownloaded { get; set; }
        private readonly RedditAPI _reddit;
        private readonly ApplicationDbContext _db;
        public SelectModel(RedditAPI reddit, ApplicationDbContext db)
        {
            _reddit = reddit;
            _db = db;
        }
        public async Task OnGetAsync()
        {
            ShowType = HttpContext.Session.GetObject<SelectShowType>("ShowType");
            Nsfw = HttpContext.Session.GetObject<SelectNsfw>("Nsfw");
            SelectedPosts = HttpContext.Session.GetObject<List<Post>>("SelectedPosts");
            ShowDownloaded = HttpContext.Session.GetObject<bool>("ShowDownloaded");

            string test = HttpContext.Session.GetString("Testing");

            //HttpContext.Session.Remove("SelectedPosts");

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
            await ManageDownloadedIdsAsync();

            Posts = filterPosts(Posts);
            PostsJson = JsonConvert.SerializeObject(Posts);
        }
        public void OnGetChangeShowType() {
            int a = 5;
        }
        //Loades all already downloaded posts and removes any downloadHistory that no longer needs to be in database
        private async Task ManageDownloadedIdsAsync() {
            List<string>? downloadedIds = HttpContext.Session.GetObject<List<string>>("DownloadedIds");
            if (downloadedIds == null)
            {
                downloadedIds = new List<string>();
                List<DownloadHistory> downloadHistories = _db.downloadHistories.Where(e => e.UserId.Equals(HttpContext.Session.GetString("RedditId"))).ToList();
                downloadHistories = downloadHistories.OrderByDescending(e => e.DownloadTime).ToList();
                foreach (Post post in AllPosts)
                {
                    for (int i = 0; i < downloadHistories.Count; i++)
                    {
                        List<string> ids = downloadHistories[i].DownloadedPosts.FromJson<List<string>>();
                        if (ids.Contains(post.Id))
                        {
                            downloadedIds.Add(post.Id);
                            break;
                        }
                    }
                }
                HttpContext.Session.SetObject("DownloadedIds", downloadedIds);
            }
            DownloadedIds = downloadedIds;
        }
        public async Task<IActionResult> OnPostChangeShowTypeAsync(string showType)
        {
            /*
            Posts = HttpContext.Session.GetObject<List<Post>>("Posts");
            AllPosts = HttpContext.Session.GetObject<List<Post>>("AllPosts");
            Nsfw = HttpContext.Session.GetObject<SelectNsfw>("Nsfw");
            ShowDownloaded = HttpContext.Session.GetObject<bool>("ShowDownloaded");
            SelectedPosts = HttpContext.Session.GetObject<List<Post>>("SelectedPosts");
            await ManageDownloadedIdsAsync();

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

            Posts = filterPosts(Posts);
            PostsJson = JsonConvert.SerializeObject(Posts);
            HttpContext.Session.SetObject("ShowType", ShowType);
            */
            return Page();
        }
        public async Task<IActionResult> OnPostSelectAsync()
        {
            AllPosts = HttpContext.Session.GetObject<List<Post>>("AllPosts");
            Posts = AllPosts;
            SelectedPosts = HttpContext.Session.GetObject<List<Post>>("SelectedPosts");
            ShowType = HttpContext.Session.GetObject<SelectShowType>("ShowType");
            await ManageDownloadedIdsAsync();

            string nsfw = Request.Form["nsfw"];
            switch (nsfw) //TODO not ideal solution change in html will brake this.
            {
                case "sfw":
                    Nsfw = SelectNsfw.SFW;
                    break;
                case "nsfw":
                    Nsfw = SelectNsfw.NSFW;
                    break;
                case "both":
                    Nsfw = SelectNsfw.BOTH;
                    break;
                default:
                    throw new Exception("In select there is nsfw that does not exist.");
            }

            string downloaded = Request.Form["showDownloaded"];
            ShowDownloaded = false;
            if (downloaded == "on") ShowDownloaded = true;

            Posts = filterPosts(Posts);
            PostsJson = JsonConvert.SerializeObject(Posts);

            HttpContext.Session.SetObject("ShowDownloaded", ShowDownloaded);
            HttpContext.Session.SetObject("Posts", Posts);
            HttpContext.Session.SetObject("Nsfw", Nsfw);
            return Page();
        }
        private List<Post> filterPosts(List<Post> posts) {
            if (!ShowDownloaded) {
                posts = posts.Where(e => !DownloadedIds.Contains(e.Id)).ToList();
            }
            switch (Nsfw) {
                case SelectNsfw.SFW:
                    posts = posts.Where(e => !e.Over18).ToList();
                    break;
                case SelectNsfw.NSFW:
                    posts = posts.Where(e => e.Over18).ToList();
                    break;
                case SelectNsfw.BOTH:
                    break;
                default:
                    throw new Exception("In select there is nsfw that does not exist.");
            }
            return posts;
        }
        public IActionResult OnPost()
        {
            Posts = HttpContext.Session.GetObject<List<Post>>("Posts");
            PostsJson = JsonConvert.SerializeObject(Posts);
            AllPosts = HttpContext.Session.GetObject<List<Post>>("AllPosts");
            int a = 5;
            return Page();
        }

        public async Task<IActionResult> OnGetRefresh()
        {
            //Fetch new posts from reddit
            List<Post> posts = await _reddit.GetAllSavedPosts(HttpContext.Session.GetString("AccessToken"), HttpContext.Session.GetString("UserName"));
            HttpContext.Session.SetObject("AllPosts", posts);
            HttpContext.Session.SetObject("Posts", posts);
            AllPosts = posts;
            Posts = posts;

            Nsfw = HttpContext.Session.GetObject<SelectNsfw>("Nsfw");
            ShowDownloaded = HttpContext.Session.GetObject<bool>("ShowDownloaded");
            ShowType = HttpContext.Session.GetObject<SelectShowType>("ShowType");
            await ManageDownloadedIdsAsync();
            Posts = filterPosts(Posts);
            PostsJson = JsonConvert.SerializeObject(Posts);
            return Page();
        }
    }
}
