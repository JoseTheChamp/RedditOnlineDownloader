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
        private readonly DownloadManager _dm;
        public ProgressModel(ApplicationDbContext db, DownloadManager dm)
        {
            _db = db;
            _dm = dm;
        }
        public void OnGet()
        {
            MyDownloads = _db.Downloads.Where(e => e.User.RedditId == HttpContext.Session.GetString("RedditId")).ToList();
            MyDownloads = MyDownloads.OrderByDescending(e => e.DownloadStart).ToList();
        }
        public IActionResult OnGetDownload(int id) {
            string path = Environment.CurrentDirectory;
            OnGet();
            //TODO will have to make sure that file path will work once it will be hosted on server.
            return File(@"/DownloadableFiles/Download" + id + ".zip", "application/zip", "Download" + id + ".zip");
        }

        public async Task<IActionResult> OnGetDeleteAsync(int id) {
            if (await _dm.RemoveDownloadProcess(id))
            {
                TempData["success"] = "Download succesfully deleted.";
            }
            else {
                TempData["error"] = "Failed deletion of download.";
            }
            OnGet();
            return Page();
        }
        public async Task<IActionResult> OnGetStopAndDeleteAsync(int id)
        {
            await _dm.StopAndRemoveDownloadProcess(id);          
            TempData["success"] = "Download succesfully deleted.";
            HttpContext.Session.Remove("DownloadedIds");
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
