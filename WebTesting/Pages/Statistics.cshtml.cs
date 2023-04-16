using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuGet.Protocol;
using System.Runtime.Intrinsics.Arm;
using WebTesting.Models;
using WebTesting.Services;

namespace WebTesting.Pages
{
    public class StatisticsModel : PageModel
    {
        public int sRegisteredUsers { get; set; }
        public int sActiveDownloads { get; set; }
        public int sPosts { get; set; }
        public int uPosts { get; set; }
        public int sDownloads { get; set; }
        public int uDownloads { get; set; }
        public double sPostsPerDownload { get; set; }
        public double uPostsPerDownload { get; set; }
        public Dictionary<string, int> sDomainsAbs { get; set; }
        public Dictionary<string, int> uDomainsAbs { get; set; }
        public Dictionary<string, int> uSubredditsAbs { get; set; }
        public Dictionary<string, int> sDomainsRel { get; set; }
        public Dictionary<string, int> uDomainsRel { get; set; }
        public Dictionary<string, int> uSubredditsRel { get; set; }


        private readonly ApplicationDbContext _db;

        public StatisticsModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public void OnGet()
        {

            if (HttpContext.Session.GetString("UserName") != null)
            {
                string redditId = HttpContext.Session.GetString("RedditId");
                Statistic systemStat = _db.Statistics.FirstOrDefault(e => e.UserId == "SYSTEM");
                Statistic stat = _db.Statistics.FirstOrDefault(e => e.UserId == redditId);

                sRegisteredUsers = _db.Users.Count();
                sActiveDownloads = _db.Downloads.Count();

                sPosts = systemStat.DownloadedPosts;
                uPosts = stat.DownloadedPosts;
                sDownloads = systemStat.Downloads;
                uDownloads = stat.Downloads;
                sPostsPerDownload = Math.Round(((double)sPosts/(double)sDownloads),1);
                uPostsPerDownload = Math.Round(((double)uPosts / (double)uDownloads), 1);

                sDomainsAbs = systemStat.DomainsJson.FromJson<Dictionary<string,int>>();
                sDomainsAbs = sDomainsAbs.OrderByDescending(e => e.Value).ToDictionary(x => x.Key, x => x.Value);
                uDomainsAbs = stat.DomainsJson.FromJson<Dictionary<string, int>>();
                uDomainsAbs = uDomainsAbs.OrderByDescending(e => e.Value).ToDictionary(x => x.Key, x => x.Value);
                uSubredditsAbs = stat.SubredditsJson.FromJson<Dictionary<string, int>>();
                uSubredditsAbs = uSubredditsAbs.OrderByDescending(e => e.Value).ToDictionary(x => x.Key, x => x.Value);
            }
            else {
                Statistic systemStat = _db.Statistics.FirstOrDefault(e => e.UserId == "SYSTEM");

                sRegisteredUsers = _db.Users.Count();
                sActiveDownloads = _db.Downloads.Count();

                sPosts = systemStat.DownloadedPosts;
                sDownloads = systemStat.Downloads;
                sPostsPerDownload = Math.Round(((double)sPosts / (double)sDownloads), 1);

                sDomainsAbs = systemStat.DomainsJson.FromJson<Dictionary<string, int>>();
                sDomainsAbs.OrderByDescending(e => e.Value);
            }
        }
    }
}
