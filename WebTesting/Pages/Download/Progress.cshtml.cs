using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTesting.Models;
using WebTesting.Services;
using WebTesting.Utils;

namespace WebTesting.Pages.Download
{
    public class ProgressModel : PageModel
    {
        public List<Models.Download> MyDownloads { get; set; }
        public bool ShowFinished { get; set; }
        private readonly ApplicationDbContext _db;
        public ProgressModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public void OnGet()
        {
            ShowFinished = HttpContext.Session.GetObject<bool>("ShowFinished");
            if (ShowFinished)
            {
                MyDownloads = _db.Downloads.Where(e => e.User.RedditId == HttpContext.Session.GetString("RedditId")).ToList();
                MyDownloads = MyDownloads.OrderByDescending(e => e.DownloadStart).ToList();
            }
            else {
                MyDownloads = _db.Downloads.Where(e => e.User.RedditId == HttpContext.Session.GetString("RedditId") && !e.IsFinished).ToList();
                MyDownloads = MyDownloads.OrderByDescending(e => e.DownloadStart).ToList();
            }
        }

        public IActionResult OnPostShowFinished() {

            string showFinished = Request.Form["show"];
            if (showFinished == "true")
            {
                HttpContext.Session.SetObject("ShowFinished", true);
            }
            else {
                HttpContext.Session.SetObject("ShowFinished", false);
            }
            OnGet();
            return Page();
        }
    }
}
