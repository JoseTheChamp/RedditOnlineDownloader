using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.IO.Compression;
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
        private static readonly string DownloadPath = Environment.CurrentDirectory + "\\wwwroot\\Downloads";
        private static readonly string DownloadablePath = Environment.CurrentDirectory + "\\wwwroot\\DownloadableFiles";
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
            Directory.CreateDirectory(DownloadPath + "\\Download" + downloadId);
            using (StreamWriter sw = File.CreateText(DownloadPath + "\\Download" + downloadId + "\\List" + dp.DownloadId + ".txt"))
            {
                int interval = dp.Posts.Count / ((dp.Posts.Count / 15) + 10);
                for (int i = 0; i < dp.Posts.Count; i++)
                {
                    sw.WriteLine(dp.Posts[i].ToString());
                    await SavePost(dp.Posts[i], dp.DownloadId);
                    if (i%interval == 0 || i == dp.Posts.Count-1)
                    {
                        if (i == dp.Posts.Count - 1)
                        {
                            Download download = _db.Downloads.FirstOrDefault(e => e.Id == downloadId);
                            download.ProgressAbs = download.ProgressAbsMax;
                            download.ProgressRel = 100;
                            download.IsFinished = true;
                            download.DownloadFinished = DateTime.Now;
                            download.IsDownloadable = true;
                            _db.Downloads.Update(download);
                            await _db.SaveChangesAsync();
                        }
                        else {
                            Download download = _db.Downloads.FirstOrDefault(e => e.Id == downloadId);
                            download.ProgressAbs = i + 1;
                            download.ProgressRel = Math.Round((double)(((double)(i + 1)) / dp.Posts.Count) * 100,1);
                            _db.Downloads.Update(download);
                            await _db.SaveChangesAsync();
                        }
                    }
                }
            }
            ZipFile.CreateFromDirectory(DownloadPath + "\\Download" + downloadId, DownloadablePath + "\\Download" + downloadId + ".zip");
        }

        private async Task SavePost(Post post,int id) {
            switch (post.Domain)
            {
                case "i.redd.it":
                    await SaveImage(post,id);
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
                        SaveTextPost(post,id);
                    }
                    break;
            }
        }

        private async Task SaveImage(Post post, int id) {
            Stream stream = await client.GetStreamAsync(post.Urls[0]);
            string name = StripName(post.Title);
            using (var fileStream = File.Create(DownloadPath + "\\Download" + id + "\\" + name + Path.GetExtension(post.Urls[0])))
            {
                stream.CopyTo(fileStream);
            }
        }
        private void SaveTextPost(Post post, int id) {
            string name = StripName(post.Title);
            using (StreamWriter sw = File.CreateText(DownloadPath + "\\Download" + id + "\\" + name + ".txt"))
            {
                sw.WriteLine("Title: " + post.Title);
                sw.WriteLine("Subreddit: " + post.Subreddit);
                sw.WriteLine("PermaLink: www.reddit.com" + post.PermaLink);
                sw.WriteLine("Text: \n" + post.SelfText);
            }
        }

        private async Task Debug(Post post, int id) {
            string name = StripName(post.Title);
            using (StreamWriter sw = File.CreateText(DownloadPath + "\\Download" + id + "\\" + name + ".txt"));
        }

        private String StripName(String name)
        {
            if (name.Length > 128) {
                name = name.Substring(0,125) + "...";
            }
            return string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
        }
        public async Task<bool> RemoveDownloadProcess(int id) {
            //TODO check if loggedin user is owner of deleted download process
            _db.Downloads.Remove(_db.Downloads.FirstOrDefault(e => e.Id == id));
            await _db.SaveChangesAsync();
            processes.Remove(processes.FirstOrDefault(e => e.DownloadId == id));
            try
            {
                Directory.Delete(DownloadPath + "\\Download" + id,true);
                File.Delete(DownloadablePath + "\\Download" + id + ".zip");
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public void StopAndRemoveDownloadProcess(int id) { 
        
        }
    }
}
