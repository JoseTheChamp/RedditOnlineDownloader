using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
            _dm.NewDownloadProcessAsync(user,Posts);
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
                if (resInvoked == "on") {
                    Posts.Add(PostsToChooseFrom.FirstOrDefault(e => e.Id == name));
                }
            }
            HttpContext.Session.SetObject("SelectedPosts",Posts);
            return Page();
        }
    }
}
