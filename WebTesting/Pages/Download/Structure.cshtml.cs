using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTesting.Entities;
using WebTesting.Utils;

namespace WebTesting.Pages.Download
{
    public class StructureModel : PageModel
    {
        public List<Post> Posts { get; set; }
        public void OnGet()
        {
            if (HttpContext.Session.GetString("SelectedPosts") != null) {
                Posts = HttpContext.Session.GetObject<List<Post>>("SelectedPosts");
            }
        }

        public IActionResult OnGetDownload() {
            //TODO Start download procces.
            //TODO if success.
            TempData["success"] = "Download started.";
            return File(@"/DownloadableFiles/Images.zip","application/zip","HokusPokus.zip");
        }
        public IActionResult OnGetDownloadAlt()
        {
            //TODO Start download procces.
            //TODO if success.
            TempData["success"] = "Download started.";
            return RedirectToPage("../Index");
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
                if (resInvoked == "on") {
                    Posts.Add(PostsToChooseFrom.FirstOrDefault(e => e.Id == name));
                }
            }
            HttpContext.Session.SetObject("SelectedPosts",Posts);
            return Page();
        }
    }
}
