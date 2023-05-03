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

        /// <summary>
        /// Fetches all downloads of the user and orders it by date.
        /// </summary>
        public void OnGet()
        {
            MyDownloads = _db.Downloads.Where(e => e.User.RedditId == HttpContext.Session.GetString("RedditId")).ToList();
            MyDownloads = MyDownloads.OrderByDescending(e => e.DownloadStart).ToList();
            //if (MyDownloads.Count == 0) MyDownloads = null;
        }
        /// <summary>
        /// Downloads the finished zip file.
        /// </summary>
        /// <param name="id">id of donwload, that is to be downloaded.</param>
        /// <returns>Finished zip file.</returns>
        public IActionResult OnGetDownload(int id) {
            //TODO zkontrolovat autora pozadavku
            string path = Environment.CurrentDirectory;
            OnGet();
            return File(@"/DownloadableFiles/Download" + id + ".zip", "application/zip", "Download" + id + ".zip");
        }

        /// <summary>
        /// Removes finished download.
        /// </summary>
        /// <param name="id">Id of download, that is to be removed.</param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetDeleteAsync(int id) {
            //TODO check if id is user's
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
        /// <summary>
        /// Removes running download process.
        /// </summary>
        /// <param name="id">Id of download, that is to be removed.</param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetStopAndDeleteAsync(int id)
        {
            await _dm.StopAndRemoveDownloadProcess(id);          
            TempData["success"] = "Download succesfully deleted.";
            HttpContext.Session.Remove("DownloadedIds");
            OnGet();
            return Page();
        }
    }
}
