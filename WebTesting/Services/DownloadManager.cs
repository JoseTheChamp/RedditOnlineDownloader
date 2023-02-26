using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Security.Policy;
using WebTesting.Entities;

namespace WebTesting.Services
{
    public class DownloadManager
    {
        List<DownloadProcess> processes = new List<DownloadProcess>();
        private HttpClient client= new HttpClient();
        private readonly RedditAPI _reddit;
        public class DownloadProcess{
            public int DownloadId { get; set; }
            public int Done { get; set; }
            public List<Post> Posts { get; set; }

            public Thread Thread { get; set; }
        }

        public DownloadManager(RedditAPI reddit)
        {
            _reddit = reddit;
        }

        public void NewDownloadProcess(int downloadId,List<Post> posts) {
            Thread thread = new Thread(() => {
                DoWork(downloadId);
            });
            DownloadProcess dp = new DownloadProcess { DownloadId = downloadId, Done = 0, Posts = posts, Thread = thread };
            processes.Add(dp);
            dp.Thread.Start();
        }

        private async void DoWork(int downloadId) {
            DownloadProcess dp = processes.FirstOrDefault(e => e.DownloadId == downloadId);
            for (int i = 0; i < dp.Posts.Count; i++)
            {
                await SavePost(dp.Posts[i]);
            }
        }

        private async Task SavePost(Post post) {
            switch (post.Domain)
            {
                case "i.redd.it":
                    SaveImage(post);
                    break;
                case "v.redd.it":
                    Debug(post);
                    break;
                case "i.imgur.com":
                    Debug(post);
                    break;
                default:
                    Debug(post);
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

        private async Task Debug(Post post) {
            string name = StripName(post.Title);
            using (StreamWriter sw = File.CreateText(@"C:\Temp\Outputs\Images\" + name + ".txt"));
        }

        private String StripName(String name)
        {
            //TODO remove ; , . and so on.
            return name;
        }
    }
}
