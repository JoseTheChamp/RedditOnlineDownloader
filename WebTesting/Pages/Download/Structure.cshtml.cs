using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using WebTesting.Entities;
using WebTesting.Entities.Enums;
using WebTesting.Models;
using WebTesting.Services;
using WebTesting.Utils;

namespace WebTesting.Pages.Download
{
    public class StructureModel : PageModel
    {
        public List<Post> Posts { get; set; }
        private readonly ApplicationDbContext _db;
        public string PostsJson { get; set; }
        private readonly DownloadManager _dm;

        public Numbering Numbering { get; set; }
        public bool SubredditName { get; set; }
        public bool DomainName { get; set; }
        public bool NamePriorityIsSubreddit { get; set; }
        public int Title { get; set; }
        public bool SubredditFolder { get; set; }
        public bool DomainFolder { get; set; }
        public bool FolderPriorityIsSubreddit { get; set; }
        public bool Empty { get; set; }
        public bool Split { get; set; }

        public List<Template> Templates { get; set; }
        public string TemplatesJson { get; set; }
        public int SelectedTemplate { get; set; }

        public StructureModel(ApplicationDbContext db, DownloadManager dm)
        {
            _db = db;
            _dm = dm;
        }

        //should not be used
        public void OnGet()
        {
            if (HttpContext.Session.GetString("SelectedPosts") != null) {
                Posts = HttpContext.Session.GetObject<List<Post>>("SelectedPosts");
            }
        }

        /// <summary>
        /// This method will launch when user clicks at download.
        /// </summary>
        /// <returns>Rediresct to index page.</returns>
        public async Task<IActionResult> OnPostDownload() {
            string saveToTemplate = null;
            //going throug each form elemnt and doing corresponding action
            foreach (string key in Request.Form.Keys)
            {
                string value = Request.Form[key];
                switch (key)
                {
                    case "numbering":
                        switch (value)
                        {
                            case "ids":
                                Numbering = Numbering.Ids;
                                break;
                            case "standard":
                                Numbering = Numbering.Standard;
                                break;
                            case "none":
                                Numbering = Numbering.None;
                                break;
                            default:
                                break;
                        }
                        break;
                    case "subredditName":
                        if (value == "on") SubredditName = true;
                        break;
                    case "domainName":
                        if (value == "on") DomainName = true;
                        break;
                    case "priorityName":
                        if (value == "subredditPriorityName") NamePriorityIsSubreddit = true;
                        break;
                    case "title":
                        Title = Int32.Parse(value);
                        break;
                    case "subredditFolder":
                        if (value == "on") SubredditFolder = true;
                        break;
                    case "domainFolder":
                        if (value == "on") DomainFolder = true;
                        break;
                    case "priorityFolder":
                        if (value == "subredditPriorityFolder") FolderPriorityIsSubreddit = true;
                        break;
                    case "empty":
                        if (value == "on") Empty = true;
                        break;
                    case "split":
                        if (value == "on") Split = true;
                        break;
                    case "templatesSelect":
                        if (value != "none")
                        {
                            HttpContext.Session.SetObject("ChosenTemplate", value);
                            saveToTemplate = value;
                        }
                        break;
                    default:
                        break;
                }
            }

            string redditId = HttpContext.Session.GetString("RedditId");

            //Create download parameters for passing into download function
            DownloadParameters downloadParameters = new DownloadParameters(
            Numbering,
            SubredditName,
            DomainName,
            NamePriorityIsSubreddit,
            Title,
            SubredditFolder,
            DomainFolder,
            FolderPriorityIsSubreddit,
            Empty,
            Split);

            //Save changes to templates if required
            if (saveToTemplate != null)
            {
                string jsonTemplate = HttpContext.Session.GetObject<string>("ChosenTemplate");
                dynamic templateParsed = JObject.Parse(jsonTemplate);
                int id = templateParsed.Id;
                Template template = _db.Templates.FirstOrDefault(p => p.UserId == redditId && p.Id == id);
                template.Numbering = Numbering.ToString().ToLower();
                template.SubredditName = SubredditName;
                template.DomainName = DomainName;
                template.PriorityName = NamePriorityIsSubreddit;
                template.SubredditFolder= SubredditFolder;
                template.DomainFolder = DomainFolder;
                template.PriorityFolder = FolderPriorityIsSubreddit;
                template.Empty = Empty;
                template.Split = Split;
                template.Title = Title;
                _db.Templates.Update(template);
                await _db.SaveChangesAsync();
            }

            int userDbId = _db.Users.FirstOrDefault(e => e.RedditId == redditId).Id;
            int numberOfUnfinishedDownloads = _db.Downloads.Where(e => e.User.Id == userDbId && e.IsDownloadable == false).ToList().Count;
            if (numberOfUnfinishedDownloads < 3)
            {
                //Creating download process, restaritng downoaded ids, by removing it, signaling success
                User user = _db.Users.FirstOrDefault(e => e.RedditId == redditId);
                Posts = HttpContext.Session.GetObject<List<Post>>("SelectedPosts");
                PostsJson = JsonConvert.SerializeObject(Posts);
                string downloadName = await _dm.NewDownloadProcessAsync(user, Posts, HttpContext.Session.GetObject<List<Post>>("AllPosts").Select(p => p.Id).ToList(), downloadParameters);
                HttpContext.Session.Remove("DownloadedIds");
                TempData["success"] = "\"" + downloadName + "\" succesfully started. You can see the progress at \"Progress\" page.";
                return RedirectToPage("../Index");
            }
            else {
                //Too many downloads, returning to index page
                User user = _db.Users.FirstOrDefault(e => e.RedditId == redditId);
                Posts = HttpContext.Session.GetObject<List<Post>>("SelectedPosts");
                PostsJson = JsonConvert.SerializeObject(Posts);
                HttpContext.Session.Remove("DownloadedIds");
                TempData["error"] = "There are already 3 running downloads. You can see the progress at \"Progress\" page.";
                return RedirectToPage("../Index");
            }
        }

        /// <summary>
        /// Launches when user clicked next stage at select screen.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostAsync()
        {
            var form = Request.Form;
            Posts = new List<Post>();
            string saveToTemplate = "";
            List<Post> PostsToChooseFrom = HttpContext.Session.GetObject<List<Post>>("AllPosts");
            HttpContext.Session.SetObject("ShowDownloaded", false);
            HttpContext.Session.SetObject("GroupBySubreddits", false);
            //Going through all form elements and doing corresponding actions
            foreach (string name in form.Keys)
            {
                var res = form[name].ToString;
                var resInvoked = res.Invoke();
                if (resInvoked == "on" && name != "showDownloaded" && name != "groupBySubreddits") {
                    Posts.Add(PostsToChooseFrom.FirstOrDefault(e => e.Id == name));
                    continue;
                }
                switch (name)
                {
                    case "showDownloaded":
                        HttpContext.Session.SetObject("ShowDownloaded", true);
                        break;
                    case "nsfw":
                        switch (resInvoked)
                        {
                            case "sfw":
                                HttpContext.Session.SetObject("Nsfw", SelectNsfw.SFW);
                                break;
                            case "nsfw":
                                HttpContext.Session.SetObject("Nsfw", SelectNsfw.NSFW);
                                break;
                            case "both":
                                HttpContext.Session.SetObject("Nsfw", SelectNsfw.BOTH);
                                break;
                            default:
                                break;
                        }
                        break;
                    case "groupBySubreddits":
                        HttpContext.Session.SetObject("GroupBySubreddits", true);
                        break;
                    case "multipleSelect[]":
                        HttpContext.Session.SetObject("DomainsForm", Request.Form["multipleSelect[]"].ToList());
                        break;
                    case "templatesSelect":
                        if (resInvoked != "none") {
                            HttpContext.Session.SetObject("ChosenTemplate", resInvoked);
                            saveToTemplate = resInvoked;
                        }
                        break;
                    default:
                        break;
                }
            }

            //If needed save to template in database
            if (saveToTemplate != "")
            {
                dynamic templateParsed = JObject.Parse(saveToTemplate);
                string id = templateParsed.Id;
                Template template = _db.Templates.FirstOrDefault(t => t.Id == Int32.Parse(id));
                template.ShowDownloaded = HttpContext.Session.GetObject<bool>("ShowDownloaded");
                template.GroupBySubreddit = HttpContext.Session.GetObject<bool>("GroupBySubreddits");
                template.DomainsForm = HttpContext.Session.GetObject<List<string>>("DomainsForm").ToJson();
                template.Nsfw = HttpContext.Session.GetObject<SelectNsfw>("Nsfw").ToString().ToLower();
                _db.Templates.Update(template);
                await _db.SaveChangesAsync();
            }
            else {
                HttpContext.Session.Remove("ChosenTemplate");
            }

            //Fetch templates
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

            HttpContext.Session.SetObject("SelectedPosts",Posts);
            PostsJson = JsonConvert.SerializeObject(Posts);
            return Page();
        }

        //Function creating new template in db
        public IActionResult OnGetNewTemplate(string name, string numbering, bool subName, bool domName, bool prioName, int title, bool subFol, bool domFol, bool prioFol, bool empty, bool split)
        {
            Templates = _db.Templates.Where(p => p.UserId == HttpContext.Session.GetString("RedditId")).ToList();

            if (!Templates.Select(t => t.Name).ToList().Contains(name))
            {
                Template tmp = new Template(0, 
                    HttpContext.Session.GetString("RedditId"), 
                    name,
                    HttpContext.Session.GetObject<bool>("ShowDownloaded"),
                    HttpContext.Session.GetObject<bool>("GroupBySubreddits"),
                    HttpContext.Session.GetObject<SelectNsfw>("Nsfw").ToString().ToLower(),
                    HttpContext.Session.GetString("DomainsForm"),
                    numbering,
                    subName,
                    domName,
                    prioName,
                    title,
                    subFol,
                    domFol,
                    prioFol,
                    empty,
                    split
                );
                var task = SaveChangesNew(tmp);
                Template result = task.Result;
                HttpContext.Session.SetObject("ChosenTemplate", result.ToJson());
                return new JsonResult(result.ToJson());
            }
            return StatusCode(422);
        }

        //Function removing template from db
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

        //Helper functions
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
