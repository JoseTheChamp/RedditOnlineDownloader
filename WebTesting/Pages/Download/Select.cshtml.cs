using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NuGet.Protocol;
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

        /// <summary>
        /// Method that launhes on the first visit of the Select. Fetches all the data for JS.
        /// </summary>
        /// <returns></returns>
        public async Task OnGetAsync()
        {
            //Getting sellected posts from session. If fetched remove. - better responsiveness
            SelectedPosts = HttpContext.Session.GetObject<List<Post>>("SelectedPosts");
            if (SelectedPosts != null) {
                List<string> selectedIds = SelectedPosts.Select(x => x.Id).ToList();
                SelectedIdsJson = JsonConvert.SerializeObject(selectedIds);
                HttpContext.Session.Remove("SelectedPosts");
            }

            //Fetching previously used settings
            ShowDownloaded = HttpContext.Session.GetObject<bool>("ShowDownloaded");
            Nsfw = HttpContext.Session.GetObject<SelectNsfw>("Nsfw");
            DomainsForm = HttpContext.Session.GetObject<List<string>>("DomainsForm");
            GroupBySubreddit = HttpContext.Session.GetObject<bool>("GroupBySubreddits");

            //Fetching templates
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

            //Getting all posts from session. (Inicialized in Loading)
            AllPosts = HttpContext.Session.GetObject<List<Post>>("AllPosts");

            //Assigning the list of ids, corresponding to which posts were already downloaded.
            await ManageDownloadedIdsAsync();

            //Creating list of all domains in downloaded posts
            Domains = AllPosts.Select(e => e.Domain).Distinct().ToList();
            HttpContext.Session.SetObject("Domains", Domains);
            if (DomainsForm == null) DomainsForm = Domains;

            PostsJson = JsonConvert.SerializeObject(AllPosts);
        }

        /// <summary>
        /// Load's all already downloaded posts and removes any downloadHistory that no longer needs to be in database
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Refreshes posts. Calls redditAPI aervice to fetch all posts.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetRefresh()
        {
            HttpContext.Session.Remove("AllPosts");
            HttpContext.Session.Remove("SelectedPosts");
            return RedirectToPage("/Download/Loading");
            //Fetch new posts from reddit
            /*List<Post> posts = await _reddit.GetAllSavedPosts(HttpContext.Session.GetString("AccessToken"), HttpContext.Session.GetString("UserName"));
            HttpContext.Session.SetObject("AllPosts", posts);
            AllPosts = posts;

            //Fetching previously used settings
            ShowDownloaded = HttpContext.Session.GetObject<bool>("ShowDownloaded");
            Nsfw = HttpContext.Session.GetObject<SelectNsfw>("Nsfw");
            DomainsForm = HttpContext.Session.GetObject<List<string>>("DomainsForm");
            GroupBySubreddit = HttpContext.Session.GetObject<bool>("GroupBySubreddits");
            Templates = _db.Templates.Where(p => p.UserId == HttpContext.Session.GetString("RedditId")).ToList();

            //Fetching templates
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

            //Assigning the list of ids, corresponding to which posts were already downloaded.
            await ManageDownloadedIdsAsync();

            //Creating list of all domains in downloaded posts
            Domains = new List<string>(); //TDOD linq groupby a select
            foreach (Post post in AllPosts)
            {
                if (!Domains.Contains(post.Domain))
                {
                    Domains.Add(post.Domain);
                }
            }
            if (DomainsForm == null) DomainsForm = Domains;
            PostsJson = JsonConvert.SerializeObject(AllPosts);
            return Page();*/
        }
        public IActionResult OnGetNewTemplate(string name, bool show, bool group, string domains, string nsfw)
        {
            Templates = _db.Templates.Where(p => p.UserId == HttpContext.Session.GetString("RedditId")).ToList();
            if (!Templates.Select(t => t.Name).ToList().Contains(name))
            {
                Template tmp = new Template(0, HttpContext.Session.GetString("RedditId"), name, show, group, nsfw, domains, "ids", false, false, true, 60, true, false, true, true, true);
                var task = SaveChangesNew(tmp);
                Template result = task.Result;
                HttpContext.Session.SetObject("ChosenTemplate", result.ToJson());
                return new JsonResult(result.ToJson());
            }
            return StatusCode(422);
        }
        public IActionResult OnGetDeleteTemplate(int id)
        {
            Templates = _db.Templates.Where(p => p.UserId == HttpContext.Session.GetString("RedditId")).ToList();
            Template template = Templates.FirstOrDefault(t => t.Id == id);
            if (template != null)
            {
                var task = SaveChangesDelete(template);
                bool result = task.Result;
                HttpContext.Session.Remove("ChosenTemplate");
                return new JsonResult("");
            }
            return StatusCode(422);
        }
        
        //Helper methods
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
