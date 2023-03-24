using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
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
        public bool Split { get; set; }

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

        public IActionResult OnGetDownload() {
            //TODO Start download procces.
            //TODO if success.
            User user = _db.Users.FirstOrDefault(e => e.RedditId == HttpContext.Session.GetString("RedditId"));
            Posts = HttpContext.Session.GetObject<List<Post>>("SelectedPosts");
            PostsJson = JsonConvert.SerializeObject(Posts);
            _dm.NewDownloadProcessAsync(user,Posts,HttpContext.Session.GetObject<List<Post>>("AllPosts").Select(p => p.Id).ToList());
            HttpContext.Session.Remove("DownloadedIds");
            TempData["success"] = "Download succesfully started. You can see the progress at \"Progress\" page.";
            return RedirectToPage("../Index");
            
        }
        public async Task<IActionResult> OnPostStructureAsync()
        {
            foreach (string key in Request.Form.Keys)
            {
                string value = Request.Form[key];
                switch (key) {
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
                    case "split":

                        break;
                    default:
                        break;
                }
            }

            Posts = HttpContext.Session.GetObject<List<Post>>("SelectedPosts");
            PostsJson = JsonConvert.SerializeObject(Posts);
            return Page();
        }


            public IActionResult OnPost()
        {
            var form = Request.Form;
            Posts = new List<Post>();
            List<Post> PostsToChooseFrom = HttpContext.Session.GetObject<List<Post>>("Posts");
            foreach (string name in form.Keys)
            {
                var res = form[name].ToString;
                var resInvoked = res.Invoke();
                if (resInvoked == "on" && name != "showDownloaded" && name != "selectAll") { //TODO bad design - based on names in htlm checkboxes on/off in select
                    Posts.Add(PostsToChooseFrom.FirstOrDefault(e => e.Id == name));
                }
            }
            HttpContext.Session.SetObject("SelectedPosts",Posts);
            PostsJson = JsonConvert.SerializeObject(Posts);
            return Page();
        }
    }
}
