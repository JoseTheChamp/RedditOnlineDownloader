using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.Security.Policy;
using WebTesting.Entities;
using WebTesting.Models;

namespace WebTesting.Services
{
    public class DownloadManager
    {
        List<DownloadProcess> processes = new List<DownloadProcess>();
        private HttpClient client= new HttpClient();
        private readonly RequestDelegate _next;
        private readonly ApplicationDbContext _db;
        public class DownloadProcess{
            public int DownloadId { get; set; }
            public List<Post> Posts { get; set; }
            public Thread Thread { get; set; }
        }

        public DownloadManager(ApplicationDbContext db)
        {
            _db = db;
        }

        public void NewDownloadProcess(User user, List<Post> posts) {
            Models.Download download = new Models.Download(
                0,
                posts.Count,
                DateTime.Now,
                user
                );
            _db.Downloads.AddAsync(download);
            _db.SaveChanges();

            Thread thread = new Thread(() => {
                DoWork(download.Id);
            });
            DownloadProcess dp = new DownloadProcess { DownloadId = download.Id, Posts = posts, Thread = thread };
            processes.Add(dp);
            dp.Thread.Start();
        }

        private async void DoWork(int downloadId)
        {
            DownloadProcess dp = processes.FirstOrDefault(e => e.DownloadId == downloadId);
            using (StreamWriter sw = File.CreateText(@"C:\Temp\Outputs\Images\List" + dp.DownloadId + ".txt"))
            {
                int report = 1;
                for (int i = 0; i < dp.Posts.Count; i++)
                {
                    sw.WriteLine(dp.Posts[i].ToString());
                    await SavePost(dp.Posts[i], dp.DownloadId);
                    if (i/dp.Posts.Count >= report/10 || i == dp.Posts.Count-1)
                    {
                        if (i == dp.Posts.Count - 1)
                        {
                            Download download = _db.Downloads.FirstOrDefault(e => e.Id == downloadId);
                            download.ProgressAbs = download.ProgressAbsMax;
                            download.ProgressRel = 100;
                            download.IsFinished = true;
                            download.DownloadFinished = DateTime.Now;
                            _db.Downloads.Update(download);
                            await _db.SaveChangesAsync();
                        }
                        else {
                            report++;
                            Download download = _db.Downloads.FirstOrDefault(e => e.Id == downloadId);
                            download.ProgressAbs = i + 1;
                            download.ProgressRel = (double)(((double)(i + 1)) / dp.Posts.Count) * 100;
                            _db.Downloads.Update(download);
                            await _db.SaveChangesAsync();
                        }
                    }
                }
            }
        }

        private async Task SavePost(Post post,int id) {
            switch (post.Domain)
            {
                case "i.redd.it":
                    await SaveImage(post);
                    break;
                case "v.redd.it":
                    //Debug(post,id);
                    break;
                case "i.imgur.com":
                    //Debug(post,id);
                    break;
                default:
                    if (post.Domain.StartsWith("self"))
                    {
                        SaveTextPost(post);
                    }
                    break;
            }
        }

        private async Task SaveImage(Post post) {
            Stream stream = await client.GetStreamAsync(post.Urls[0]);
            string name = StripName(post.Title);
            using (var fileStream = File.Create(@"C:\Temp\Outputs\Images\" + name + Path.GetExtension(post.Urls[0])))
            {
                stream.CopyTo(fileStream);
            }
        }
        private void SaveTextPost(Post post) {
            string name = StripName(post.Title);
            using (StreamWriter sw = File.CreateText(@"C:\Temp\Outputs\Images\" + name + ".txt"))
            {
                sw.WriteLine("Title: " + post.Title);
                sw.WriteLine("Subreddit: " + post.Subreddit);
                sw.WriteLine("PermaLink: www.reddit.com" + post.PermaLink);
                sw.WriteLine("Text: \n" + post.SelfText);
            }
        }



        private async Task Debug(Post post) {
            string name = StripName(post.Title);
            using (StreamWriter sw = File.CreateText(@"C:\Temp\Outputs\Images\" + name + ".txt"));
        }

        private String StripName(String name)
        {
            if (name.Length > 128) {
                name = name.Substring(0,125) + "...";
            }
            return string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
