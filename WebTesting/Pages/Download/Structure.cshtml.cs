using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using WebTesting.Entities;
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
        /*public IActionResult OnGetDownloadAlt()
        {
            //TODO Start download procces.
            //TODO if success.

            TempData["success"] = "Download started.";
            return File(@"/DownloadableFiles/Images.zip", "application/zip", "PPPPPP.zip");
            
        }*/
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
