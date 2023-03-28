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
using System.Diagnostics;

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
        public List<Template> Templates { get; set; }
        public string TemplatesJson { get; set; }
        public int SelectedTemplate { get; set; }

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
            string a = HttpContext.Session.GetString("RedditId");
            List<Template> temps = _db.Templates.ToList();

            Templates = _db.Templates.Where(p => p.UserId == HttpContext.Session.GetString("RedditId")).ToList();
            TemplatesJson = Templates.ToJson();
            if (HttpContext.Session.GetObject<string>("ChosenTemplate") != null)
            {
                string jsonTemplate = HttpContext.Session.GetObject<string>("ChosenTemplate");
                dynamic templateParsed = JObject.Parse(jsonTemplate);
                SelectedTemplate = templateParsed.Id;
            }
            else {
                SelectedTemplate = 0;
            }

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
            Templates = _db.Templates.Where(p => p.UserId == HttpContext.Session.GetString("RedditId")).ToList();
            TemplatesJson = Templates.ToJson();
            if (HttpContext.Session.GetObject<string>("ChosenTemplate") != null)
            {
                string jsonTemplate = HttpContext.Session.GetObject<string>("ChosenTemplate");
                dynamic templateParsed = JObject.Parse(jsonTemplate);
                SelectedTemplate = templateParsed.Id;
            }
            else
            {
                SelectedTemplate = 0;
            }

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
        public IActionResult OnGetNewTemplate(string name)
        {
            int a = 5;
            Templates = _db.Templates.Where(p => p.UserId == HttpContext.Session.GetString("RedditId")).ToList();
            if (!Templates.Select(t => t.Name).ToList().Contains(name))
            {
                Template tmp = new Template(0, HttpContext.Session.GetString("RedditId"), name);
                var task = SaveChangesNew(tmp);
                Template result = task.Result;

                return new JsonResult(result.ToJson());
            }
            return StatusCode(422);
        }
        public IActionResult OnGetDeleteTemplate(int id)
        {
            int a = 5;
            Templates = _db.Templates.Where(p => p.UserId == HttpContext.Session.GetString("RedditId")).ToList();
            Template template = null;
            if (template != null) { //TODO maybe breaks
                int vfvf = 5;
            }
            template = Templates.FirstOrDefault(t => t.Id == id);
            if (template != null)
            {
                var task = SaveChangesDelete(template);
                bool result = task.Result;
                return new JsonResult("");
            }
            return StatusCode(422);
        }
        private async Task<Template> SaveChangesNew(Template tmp)
        {
            _db.Templates.Add(tmp);
            await _db.SaveChangesAsync();
            return tmp;
        }
        private async Task<bool> SaveChangesDelete(Template tmp)
        {
            _db.Templates.Remove(tmp);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
