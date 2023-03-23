﻿using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;
using NuGet.Protocol;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Policy;
using System.Threading;
using System.Timers;
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
        private System.Timers.Timer RemoveTimer;
        public class DownloadProcess{
            public int DownloadId { get; set; }
            public List<Post> Posts { get; set; }
            public Thread Thread { get; set; }
            public string UserId { get; set; }
            public CancellationTokenSource TokenSource { get; set; }

            public DownloadHistory DownloadHistory;
        }

        public DownloadManager(ApplicationDbContext db)
        {
            _db = db;
        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            List<Download> downloads = _db.Downloads.ToList();
            foreach (Download download in downloads)
            {
                if (DateTime.Now.Subtract((DateTime)download.DownloadFinished).TotalHours > 72) //TODO Parametr
                {
                    RemoveDownloadProcess(download.Id);
                }
            }
        }
        public async Task NewDownloadProcessAsync(User user, List<Post> posts, List<string> AllIds) {
            if (RemoveTimer == null) {
                RemoveTimer = new System.Timers.Timer(180000); //TODO Parametr - set to something like 1800000 add 0
                RemoveTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                RemoveTimer.Start();
            }
            Models.Download download = new Models.Download(
                0,
                posts.Count,
                DateTime.Now,
                user
                );
            _db.Downloads.AddAsync(download);
            await _db.SaveChangesAsync();

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            Thread thread = new Thread(() => {
                DoWork(download.Id, tokenSource.Token, AllIds);
            });
            DownloadProcess dp = new DownloadProcess { DownloadId = download.Id, Posts = posts, Thread = thread, UserId = user.RedditId, TokenSource = tokenSource};
            processes.Add(dp);

            //Save Download History
            List<string> ids = new List<string>();
            foreach (Post post in dp.Posts)
            {
                ids.Add(post.Id);
            }
            var a = ids.ToJson();
            DownloadHistory dh = new DownloadHistory(0, dp.UserId, ids.ToJson(), DateTime.Now);
            _db.downloadHistories.Add(dh);
            await _db.SaveChangesAsync();
            dp.DownloadHistory = dh;
            dp.Thread.Start();
        }

        private async void DoWork(int downloadId, CancellationToken token, List<string> AllIds)
        {
            DownloadProcess dp = processes.FirstOrDefault(e => e.DownloadId == downloadId);
            Directory.CreateDirectory(DownloadPath + "\\Download" + downloadId);
            using (StreamWriter sw = File.CreateText(DownloadPath + "\\Download" + downloadId + "\\List" + dp.DownloadId + ".txt"))
            {
                int interval = dp.Posts.Count / ((dp.Posts.Count / 15) + 10);
                if (interval == 0) interval = 1;
                for (int i = 0; i < dp.Posts.Count; i++)
                {
                    if (token.IsCancellationRequested) {
                        Debug.WriteLine("CLOSE---------------------------------");
                        sw.Dispose();
                        sw.Close();
                        break;
                    }
                    try
                    {
                        sw.WriteLine(dp.Posts[i].ToString());
                        await SavePost(dp.Posts[i], dp.DownloadId);
                        if (i % interval == 0 || i == dp.Posts.Count - 1)
                        {
                            if (i == dp.Posts.Count - 1)
                            {
                                Download download = _db.Downloads.FirstOrDefault(e => e.Id == downloadId);
                                if (download != null)
                                {
                                    download.ProgressAbs = download.ProgressAbsMax;
                                    download.ProgressRel = 100;
                                    download.IsFinished = true;
                                    download.DownloadFinished = DateTime.Now;
                                    _db.Downloads.Update(download);
                                    await _db.SaveChangesAsync();
                                }
                            }
                            else
                            {
                                Download download = _db.Downloads.FirstOrDefault(e => e.Id == downloadId);
                                if (download != null) {
                                    download.ProgressAbs = i + 1;
                                    download.ProgressRel = Math.Round((double)(((double)(i + 1)) / dp.Posts.Count) * 100, 1);
                                    _db.Downloads.Update(download);
                                    await _db.SaveChangesAsync();
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            if (!token.IsCancellationRequested) {
                ZipFile.CreateFromDirectory(DownloadPath + "\\Download" + downloadId, DownloadablePath + "\\Download" + downloadId + ".zip");
                try
                {
                    Directory.Delete(DownloadPath + "\\Download" + downloadId, true);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                //Allow files to be downloaded
                Download downloadDownloadable = _db.Downloads.FirstOrDefault(e => e.Id == downloadId);
                downloadDownloadable.IsDownloadable = true;
                _db.Downloads.Update(downloadDownloadable);

                //Check number of downloads if above x then remove oldest.
                List<Download> downloads = _db.Downloads.Where(e => e.User.RedditId == dp.UserId).ToList();
                if (downloads.Count > 5)
                { //TODO Parameter 
                    Download download = downloads.OrderBy(e => e.DownloadFinished).FirstOrDefault();
                    await RemoveDownloadProcess(download.Id);
                }

                //Removal of needless downloadHistories
                List<DownloadHistory> downloadHistories = _db.downloadHistories.Where(e => e.UserId.Equals(dp.UserId)).ToList();
                downloadHistories = downloadHistories.OrderByDescending(e => e.DownloadTime).ToList();
                List<int> hits = new List<int>(new int[downloadHistories.Count]);
                foreach (string id in AllIds)
                {
                    for (int i = 0; i < downloadHistories.Count; i++)
                    {
                        List<string> ids = downloadHistories[i].DownloadedPosts.FromJson<List<string>>();
                        if (ids.Contains(id))
                        {
                            hits[i]++;
                            break;
                        }
                    }
                }
                for (int i = 0; i < hits.Count; i++)
                {
                    if (hits[i] == 0)
                    {
                        _db.downloadHistories.Remove(downloadHistories[i]);
                        await _db.SaveChangesAsync();
                    }
                }

                //Save changes to database
                await _db.SaveChangesAsync();
            }
        }

        private async Task SavePost(Post post,int id) {
            switch (post.Domain)
            {
                //Images
                case "i.redd.it":
                    await SaveImage(post,id);
                    break;
                case "i.imgur.com":
                    await SaveImage(post, id);
                    break;
                case "reddit.com":
                    await SaveMultipleImages(post, id);
                    break;
                //Videos
                case "v.redd.it":
                    await SaveVideo(post, id);
                    break;
                case "gfycat.com":
                    await SaveVideo(post, id);
                    break;
                //txt files
                case "link":
                    SaveLinkPost(post, id);
                    break;
                case "comment":
                    SaveComment(post, id);   
                    break;
                case "text":
                    SaveTextPost(post, id);
                    break;
                //default makes ERROR txt file
                default:
                    SaveErrorPost(post, id);
                    break;
            }
        }

        private async Task SaveVideo(Post post, int id) {
            string url = post.Urls[0];
            if (url.Contains('?')) {
                url = url.Substring(0, url.IndexOf('?'));
            }
            string name = StripName(post.Title);

            int len = url.LastIndexOf('.') - url.LastIndexOf('_') - 1;
            string audioUrl = url.Remove(url.LastIndexOf('_') + 1, len);
            audioUrl = audioUrl.Insert(audioUrl.LastIndexOf('_') + 1, "audio");
            bool audioExists = false;
            try
            {
                Stream streamAudio = await client.GetStreamAsync(audioUrl);
                audioExists = true;
            }
            catch (Exception){}
            if (audioExists)
            {
                Stream streamVideoCombined = await client.GetStreamAsync(@"https://sd.redditsave.com/download.php?permalink=https://reddit.com/&video_url=" + url + "&audio_url=" + audioUrl);
                using (var fileStream = File.Create(DownloadPath + "\\Download" + id + "\\" + name + Path.GetExtension(url)))
                {
                    streamVideoCombined.CopyTo(fileStream);
                }
            }
            else {
                Stream streamVideo = await client.GetStreamAsync(url);
                using (var fileStream = File.Create(DownloadPath + "\\Download" + id + "\\" + name + Path.GetExtension(url)))
                {
                    streamVideo.CopyTo(fileStream);
                }
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
        private async Task SaveErrorPost(Post post, int id)
        {
            string name = StripName(post.Title);
            using (StreamWriter sw = File.CreateText(DownloadPath + "\\Download" + id + "\\ERROR_DOMAIN_UNRECOGNIZED_" + name + ".txt"))
            {
                sw.WriteLine("Title: " + post.Title);
                sw.WriteLine("Subreddit: " + post.Subreddit);
                sw.WriteLine("PermaLink: www.reddit.com" + post.PermaLink);
            }
        }
        private async Task SaveMultipleImages(Post post, int id)
        {
            string name = StripName(post.Title);
            for (int i = 0; i < post.Urls.Count; i++)
            {
                Stream stream = await client.GetStreamAsync(post.Urls[i]);
                using (var fileStream = File.Create(DownloadPath + "\\Download" + id + "\\" + name + "_" + i + Path.GetExtension(post.Urls[i])))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }
        private void SaveTextPost(Post post, int id) {
            string name = StripName(post.Title);
            using (StreamWriter sw = File.CreateText(DownloadPath + "\\Download" + id + "\\Text_" + name + ".txt"))
            {
                sw.WriteLine("Title: " + post.Title);
                sw.WriteLine("Subreddit: " + post.Subreddit);
                sw.WriteLine("PermaLink: www.reddit.com" + post.PermaLink);
                sw.WriteLine("Text: \n" + post.SelfText);
            }
        }

        private void SaveComment(Post post, int id)
        {
            string name = StripName(post.Title);
            using (StreamWriter sw = File.CreateText(DownloadPath + "\\Download" + id + "\\Comment_" + name + ".txt"))
            {
                sw.WriteLine("Comment: " + post.Title);
                sw.WriteLine("Subreddit: " + post.Subreddit);
                sw.WriteLine("PermaLink to a comment: www.reddit.com" + post.PermaLink);
            }
        }

        private void SaveLinkPost(Post post, int id)
        {
            string name = StripName(post.Title);
            using (StreamWriter sw = File.CreateText(DownloadPath + "\\Download" + id + "\\Link_" + name + ".txt"))
            {
                sw.WriteLine("Title: " + post.Title);
                sw.WriteLine("Subreddit: " + post.Subreddit);
                sw.WriteLine("PermaLink to a post: www.reddit.com" + post.PermaLink);
                if (post.SelfText != "")
                {
                    sw.WriteLine("Text: \n" + post.SelfText);
                }
                sw.WriteLine("Link: \n" + post.Urls[0]);
            }
        }

        private async Task DebugPost(Post post, int id) {
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
                File.Delete(DownloadablePath + "\\Download" + id + ".zip");
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public async Task StopAndRemoveDownloadProcess(int id) {

            DownloadProcess dp = processes.FirstOrDefault(e => e.DownloadId == id);
            dp.TokenSource.Cancel();
            dp.TokenSource.Dispose();
            _db.Downloads.Remove(_db.Downloads.FirstOrDefault(e => e.Id == id));
            _db.downloadHistories.Remove(dp.DownloadHistory);
            await _db.SaveChangesAsync();
            processes.Remove(dp);
            Thread thread = new Thread(() => {
                DeleteDownloadFiles(id);
            });
            thread.Start();
        }
        private void DeleteDownloadFiles(int id)
        {
            int tries = 0;
            start:
            try
            {
                Directory.Delete(DownloadPath + "\\Download" + id, true);
            }
            catch (Exception ex)
            {
                tries++;
                if (tries > 10) {
                    throw ex;
                }
                Thread.Sleep(2000);
                goto start;
            }

        }
    }
}
