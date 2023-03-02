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
            string res = HttpContext.Session.GetString("ShowFinished");
            switch (res)
            {
                case null:
                    ShowFinished = true;
                    break;
                case "true":
                    ShowFinished = true;
                    break;
                case "false":
                    ShowFinished = false;
                    break;
                default:
                    break;
            }
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
                HttpContext.Session.SetString("ShowFinished", "true");
            }
            else {
                HttpContext.Session.SetString("ShowFinished", "false");
            }
            OnGet();
            return Page();
        }
        public IActionResult OnGetDownload(int id) {
            int a = 5;
            a = id;
            OnGet();
            return Page();
        }

        public IActionResult OnGetDelete(int id) {
            int a = 5;
            OnGet();
            return Page();
        }
        public IActionResult OnGetSave(int id)
        {
            int a = 5;
            OnGet();
            return Page();
        }
    }
}
