using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using System.Diagnostics;
using System.Text.Json.Nodes;
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

        public StructureModel(ApplicationDbContext db, DownloadManager dm)
        {
            _db = db;
            _dm = dm;
        }

        public void OnGet()
        {
            if (HttpContext.Session.GetString("SelectedPosts") != null) {
                Posts = HttpContext.Session.GetObject<List<Post>>("SelectedPosts");
            }
        }

        public IActionResult OnPostDownload() {
            //TODO Start download procces.
            //TODO if success.

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
                        Title = Int32.Parse(value); //TODO somecheck? maybe do on html/js side
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
                    default:
                        break;
                }
            }
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

            User user = _db.Users.FirstOrDefault(e => e.RedditId == HttpContext.Session.GetString("RedditId"));
            Posts = HttpContext.Session.GetObject<List<Post>>("SelectedPosts");
            PostsJson = JsonConvert.SerializeObject(Posts);
            _dm.NewDownloadProcessAsync(user,Posts,HttpContext.Session.GetObject<List<Post>>("AllPosts").Select(p => p.Id).ToList(), downloadParameters);
            HttpContext.Session.Remove("DownloadedIds");
            TempData["success"] = "Download succesfully started. You can see the progress at \"Progress\" page.";
            return RedirectToPage("../Index");
            
        }
        public IActionResult OnGetDownload()
        {
            //TODO Start download procces.
            //TODO if success.
            User user = _db.Users.FirstOrDefault(e => e.RedditId == HttpContext.Session.GetString("RedditId"));
            Posts = HttpContext.Session.GetObject<List<Post>>("SelectedPosts");
            PostsJson = JsonConvert.SerializeObject(Posts);
            _dm.NewDownloadProcessAsync(user, Posts, HttpContext.Session.GetObject<List<Post>>("AllPosts").Select(p => p.Id).ToList(), null);
            HttpContext.Session.Remove("DownloadedIds");
            TempData["success"] = "Download succesfully started. You can see the progress at \"Progress\" page.";
            return RedirectToPage("../Index");

        }
        public async Task<IActionResult> OnPostStructureAsync()
        {

            Posts = HttpContext.Session.GetObject<List<Post>>("SelectedPosts");
            PostsJson = JsonConvert.SerializeObject(Posts);
            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            var form = Request.Form;
            Posts = new List<Post>();
            string saveToTemplate = "";
            List<Post> PostsToChooseFrom = HttpContext.Session.GetObject<List<Post>>("AllPosts");
            HttpContext.Session.SetObject("ShowDownloaded", false);
            HttpContext.Session.SetObject("GroupBySubreddits", false);
            foreach (string name in form.Keys)
            {
                var res = form[name].ToString;
                var resInvoked = res.Invoke();
                if (resInvoked == "on" && name != "showDownloaded" && name != "selectAll" && name != "groupBySubreddits") { //TODO bad design - based on names in htlm checkboxes on/off in select
                    Posts.Add(PostsToChooseFrom.FirstOrDefault(e => e.Id == name));
                    continue;
                }
                switch (name)
                {
                    case "showDownloaded":
                        HttpContext.Session.SetObject("ShowDownloaded", true);
                        break;
                    case "nsfw":
                        switch (resInvoked) //TODO not ideal solution change in html will brake this.
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
                                throw new Exception("In select there is nsfw that does not exist.");
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
            Templates = _db.Templates.Where(p => p.UserId == HttpContext.Session.GetString("RedditId")).ToList();
            TemplatesJson = Templates.ToJson();
            HttpContext.Session.SetObject("SelectedPosts",Posts);
            PostsJson = JsonConvert.SerializeObject(Posts);
            return Page();
        }
    }
}
