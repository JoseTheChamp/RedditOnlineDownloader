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
        public string PostsJson { get; set; }
        public List<Post> SelectedPosts { get; set; }
        public string SelectedIdsJson { get; set; }
        public List<string> DownloadedIds { get; set; }
        public string DownloadedIdsJson { get; set; }
        
        public List<string> Domains { get; set; }


        public SelectNsfw Nsfw { get; set; }
        public bool ShowDownloaded { get; set; }
        public List<string> DomainsForm { get; set; }
        public bool GroupBySubreddit { get; set; }



        private readonly RedditAPI _reddit;
        private readonly ApplicationDbContext _db;
        public SelectModel(RedditAPI reddit, ApplicationDbContext db)
        {
            _reddit = reddit;
            _db = db;
        }
        public async Task OnGetAsync()
        {
            SelectedPosts = HttpContext.Session.GetObject<List<Post>>("SelectedPosts");
            if (SelectedPosts != null) {
                List<string> selectedIds = SelectedPosts.Select(x => x.Id).ToList();
                SelectedIdsJson = JsonConvert.SerializeObject(selectedIds);
                HttpContext.Session.Remove("SelectedPosts");
            }

            ShowDownloaded = HttpContext.Session.GetObject<bool>("ShowDownloaded");
            Nsfw = HttpContext.Session.GetObject<SelectNsfw>("Nsfw");
            DomainsForm = HttpContext.Session.GetObject<List<string>>("DomainsForm");
            GroupBySubreddit = HttpContext.Session.GetObject<bool>("GroupBySubreddits");

            if (HttpContext.Session.GetString("AllPosts") != null)
            {
                AllPosts = HttpContext.Session.GetObject<List<Post>>("AllPosts");
            }
            else
            {
                List<Post> posts = await _reddit.GetAllSavedPosts(HttpContext.Session.GetString("AccessToken"), HttpContext.Session.GetString("UserName"));
                HttpContext.Session.SetObject("AllPosts", posts);
                AllPosts = posts;
            }
            await ManageDownloadedIdsAsync();

            Domains = new List<string>(); //TODO replace with linq
            foreach (Post post in AllPosts)
            {
                if (!Domains.Contains(post.Domain)) { 
                    Domains.Add(post.Domain);
                }
            }
            HttpContext.Session.SetObject("Domains", Domains);
            if (DomainsForm == null) DomainsForm = Domains;

            PostsJson = JsonConvert.SerializeObject(AllPosts);
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
            DownloadedIdsJson = JsonConvert.SerializeObject(downloadedIds);
        }
        public IActionResult OnPost()
        {
            AllPosts = HttpContext.Session.GetObject<List<Post>>("AllPosts");
            PostsJson = JsonConvert.SerializeObject(AllPosts);
            return Page();
        }

        public async Task<IActionResult> OnGetRefresh()
        {
            //Fetch new posts from reddit
            List<Post> posts = await _reddit.GetAllSavedPosts(HttpContext.Session.GetString("AccessToken"), HttpContext.Session.GetString("UserName"));
            HttpContext.Session.SetObject("AllPosts", posts);
            AllPosts = posts;

            ShowDownloaded = HttpContext.Session.GetObject<bool>("ShowDownloaded");
            Nsfw = HttpContext.Session.GetObject<SelectNsfw>("Nsfw");
            DomainsForm = HttpContext.Session.GetObject<List<string>>("DomainsForm");
            GroupBySubreddit = HttpContext.Session.GetObject<bool>("GroupBySubreddits");

            Domains = new List<string>(); //TDOD linq groupby a select
            foreach (Post post in AllPosts)
            {
                if (!Domains.Contains(post.Domain))
                {
                    Domains.Add(post.Domain);
                }
            }
            if (DomainsForm == null) DomainsForm = Domains;

            await ManageDownloadedIdsAsync();
            PostsJson = JsonConvert.SerializeObject(AllPosts);
            return Page();
        }
    }
}
