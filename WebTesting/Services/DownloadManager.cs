using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
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
            public List<string> AllIds { get; set; }
            public DownloadParameters DownloadParameters { get; set; }
            public List<string> ExistingNames { get; set; }
            public int Interval { get; set; }
            public int PostIndex { get; set; }
            public Thread Thread { get; set; }
            public string UserId { get; set; }
            public CancellationTokenSource TokenSource { get; set; }

            public DownloadHistory DownloadHistory;
        }

        public DownloadManager(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Method responsible for removing downloads that are too old.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            List<Download> downloads = _db.Downloads.ToList();
            foreach (Download download in downloads)
            {
                if (download.DownloadFinished != null) {
                    Debug.WriteLine(download.Id + "  delka: " + DateTime.Now.Subtract((DateTime)download.DownloadFinished).TotalHours);
                    if (DateTime.Now.Subtract((DateTime)download.DownloadFinished).TotalHours > 72) //TODO Parametr
                    {
                        RemoveDownloadProcess(download.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Starting of the download process.
        /// </summary>
        /// <param name="user">User which requested the download.</param>
        /// <param name="posts">Posts to download.</param>
        /// <param name="AllIds">List of id|s to determine which DownloadHistorie to remove.</param>
        /// <param name="downloadParams">Parameters that decide how the downloaded files will look.</param>
        /// <returns></returns>
        public async Task<string> NewDownloadProcessAsync(User user, List<Post> posts, List<string> AllIds, DownloadParameters downloadParams) {
            //Starting the method to remove too old posts
            if (RemoveTimer == null) { //TODO bad solution
                RemoveTimer = new System.Timers.Timer(60000); //TODO Parametr - set to something like 1800000 add 0
                RemoveTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                RemoveTimer.Start();
            }

            //Creating download row in databse
            Models.Download download = new Models.Download(
                0,
                posts.Count,
                DateTime.Now,
                user
                );
            _db.Downloads.AddAsync(download);
            await _db.SaveChangesAsync();
            
            //Initialization of required things for staritng the work and creating DownloadProcess
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            int interval = posts.Count / ((posts.Count / 15) + 10);
            if (interval == 0) interval = 1;
            DownloadProcess dp = new DownloadProcess { 
                ExistingNames = new List<string>(), 
                PostIndex = 0, 
                DownloadId = download.Id, 
                Posts = posts, 
                UserId = user.RedditId, 
                TokenSource = tokenSource, 
                AllIds = AllIds, 
                DownloadParameters = downloadParams, 
                Interval = interval};
            processes.Add(dp);
            Thread thread = new Thread(() => {
                DoWork(dp);
            });
            dp.Thread = thread;

            //Save Download History
            List<string> ids = new List<string>(); //TODO replace with LINQ
            foreach (Post post in dp.Posts)
            {
                ids.Add(post.Id);
            }
            var a = ids.ToJson();
            DownloadHistory dh = new DownloadHistory(0, dp.UserId, ids.ToJson(), DateTime.Now);
            _db.downloadHistories.Add(dh);
            await _db.SaveChangesAsync();
            dp.DownloadHistory = dh;

            //Starting the actual download itself.
            dp.Thread.Start();

            return "Download" + dp.DownloadId;
        }

        /// <summary>
        /// Method responsible for the actual downloading of the files.
        /// </summary>
        /// <param name="dp">Download procces that is currently active.</param>
        private async void DoWork(DownloadProcess dp)
        {
            string defaultPath = DownloadPath + "\\Download" + dp.DownloadId;
            Directory.CreateDirectory(defaultPath);

            //Getting list of all domains and all subreddits
            List<string> domains = new List<string>(); //TODO replace with LINQ
            foreach (Post post in dp.Posts)
            {
                if (!domains.Contains(post.Domain))
                {
                    domains.Add(post.Domain);
                }
            }
            domains.Sort();
            List<string> subreddits = new List<string>(); //TODO replace with LINQ
            foreach (Post post in dp.Posts)
            {
                if (!subreddits.Contains(post.Subreddit))
                {
                    subreddits.Add(post.Subreddit);
                }
            }
            subreddits.Sort();

            //Create two folders if nsfw/sfw split
            if (dp.DownloadParameters.Split == true) {
                List<Post> sfwPosts = dp.Posts.Where(p => p.Over18 == false).ToList();
                if (dp.DownloadParameters.Empty && !sfwPosts.IsNullOrEmpty()) {
                    //make directory and download part
                    string currentPath = defaultPath + "\\sfw";
                    Directory.CreateDirectory(currentPath);
                    if (! await SavePart(dp, sfwPosts, currentPath,domains,subreddits)) return; 
                }
                List<Post> nsfwPosts = dp.Posts.Where(p => p.Over18 == true).ToList();
                if (dp.DownloadParameters.Empty && !nsfwPosts.IsNullOrEmpty())
                {
                    //make directory and download part
                    string currentPath = defaultPath + "\\nsfw";
                    Directory.CreateDirectory(currentPath);
                    if (!await SavePart(dp, nsfwPosts, currentPath, domains, subreddits)) return;
                }
            }
            else {
                //download part
                if (!await SavePart(dp, dp.Posts, defaultPath, domains, subreddits)) return;
            }

            //Following code executes either after fully odwnloading all posts or requesting stop
            //Figure out which scenario is happening
            if (!dp.TokenSource.IsCancellationRequested) {
                //Make and save zip file
                ZipFile.CreateFromDirectory(DownloadPath + "\\Download" + dp.DownloadId, DownloadablePath + "\\Download" + dp.DownloadId + ".zip");
                try
                {
                    Directory.Delete(DownloadPath + "\\Download" + dp.DownloadId, true);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                //Allow files to be downloaded
                Download downloadDownloadable = _db.Downloads.FirstOrDefault(e => e.Id == dp.DownloadId);
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
                foreach (string id in dp.AllIds)
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
        /// <summary>
        /// saves part of the posts.
        /// </summary>
        /// <param name="dp">Download procces that is currently downloading.</param>
        /// <param name="posts">Filtered posts to be downloaded.</param>
        /// <param name="path">Path at which the post should be saved at.</param>
        /// <param name="domains">List of all domains.</param>
        /// <param name="subreddits">List of all subreddits</param>
        /// <returns>False means that cancellation was requested</returns>

        private async Task<bool> SavePart(DownloadProcess dp, List<Post> posts, string path, List<string> domains, List<string> subreddits) {
            if (dp.DownloadParameters.DomainFolder || dp.DownloadParameters.SubredditFolder)
            {
                if (dp.DownloadParameters.DomainFolder) {
                    if (dp.DownloadParameters.SubredditFolder) {
                        //both foldering
                        if (dp.DownloadParameters.FolderPriorityIsSubreddit) {
                            //subreddit priority
                            foreach (string subreddit in subreddits)
                            {
                                List<Post> filteredPosts = posts.Where(p => p.Subreddit == subreddit).ToList();
                                if (dp.DownloadParameters.Empty && !filteredPosts.IsNullOrEmpty())
                                {
                                    string currentPath = path + "\\" + subreddit.Substring(2, subreddit.Length - 2);
                                    Directory.CreateDirectory(currentPath);
                                    foreach (string domain in domains)
                                    {
                                        filteredPosts = filteredPosts.Where(p => p.Domain == domain).ToList();
                                        if (dp.DownloadParameters.Empty && !filteredPosts.IsNullOrEmpty())
                                        {
                                            currentPath = currentPath + "\\" + domain;
                                            Directory.CreateDirectory(currentPath);
                                            if (!await DownloadPosts(dp, filteredPosts, currentPath)) return false;
                                        }
                                    }
                                }
                            }
                        }
                        else {
                            //domain priority
                            foreach (string domain in domains)
                            {
                                List<Post> filteredPosts = posts.Where(p => p.Domain == domain).ToList();
                                if (dp.DownloadParameters.Empty && !filteredPosts.IsNullOrEmpty())
                                {
                                    string currentPath = path + "\\" + domain;
                                    Directory.CreateDirectory(currentPath);
                                    foreach (string subreddit in subreddits)
                                    {
                                        filteredPosts = filteredPosts.Where(p => p.Subreddit == subreddit).ToList();
                                        if (dp.DownloadParameters.Empty && !filteredPosts.IsNullOrEmpty())
                                        {
                                            currentPath = currentPath + "\\" + subreddit.Substring(2, subreddit.Length - 2);
                                            Directory.CreateDirectory(currentPath);
                                            if (!await DownloadPosts(dp, filteredPosts, currentPath)) return false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else {
                        //domain foldering
                        foreach (string domain in domains)
                        {
                            List<Post> filteredPosts = posts.Where(p => p.Domain == domain).ToList();
                            if (dp.DownloadParameters.Empty && !filteredPosts.IsNullOrEmpty())
                            {
                                string currentPath = path + "\\" + domain;
                                Directory.CreateDirectory(currentPath);
                                if (!await DownloadPosts(dp, filteredPosts, currentPath)) return false;
                            }
                        }
                    }
                }
                else {
                    //subreddits foldering
                    foreach (string subreddit in subreddits)
                    {
                        List<Post> filteredPosts = posts.Where(p => p.Subreddit == subreddit).ToList();
                        if (dp.DownloadParameters.Empty && !filteredPosts.IsNullOrEmpty())
                        {
                            string currentPath = path + "\\" + subreddit.Substring(2, subreddit.Length - 2);
                            Directory.CreateDirectory(currentPath);
                            if (!await DownloadPosts(dp, filteredPosts, currentPath)) return false;
                        }
                    }
                }
            } else {
                //No foldering
                if (!await DownloadPosts(dp, posts, path)) return false;
            }
            return true;
        }

        /// <summary>
        /// Method to download all posts passed into it.
        /// </summary>
        /// <param name="dp">Download procces that is currently downloading.</param>
        /// <param name="posts">Filtered posts to be downloaded.</param>
        /// <param name="path">Path at which the post should be saved at.</param>
        /// <returns>False means that cancellation was requested</returns>
        private async Task<bool> DownloadPosts(DownloadProcess dp, List<Post> posts, string path) {
            int indexInFolder = 1;
            dp.ExistingNames.Clear();
            string currentPath = "";
            foreach (Post post in posts)
            {
                //Check if cancellation was requested
                if (dp.TokenSource.IsCancellationRequested)
                {
                    Debug.WriteLine("CLOSE---------------------------------");
                    return false;
                }

                //Try generating name for file
                try {
                    currentPath = path + "\\" + generateName(post, dp, indexInFolder);
                } catch(Exception ex) {
                    currentPath = path + "\\ERROR";
                }

                //Download the post
                await SavePost(post, currentPath, dp.DownloadId);

                indexInFolder++;
                dp.PostIndex++;

                //Check if the interval is right for update of the download proccess.
                if (dp.PostIndex % dp.Interval == 0 || dp.PostIndex == dp.Posts.Count - 1)
                {
                    if (dp.PostIndex == dp.Posts.Count - 1)
                    {
                        Download download = _db.Downloads.FirstOrDefault(e => e.Id == dp.DownloadId);
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
                        Download download = _db.Downloads.FirstOrDefault(e => e.Id == dp.DownloadId);
                        if (download != null)
                        {
                            download.ProgressAbs = dp.PostIndex;
                            download.ProgressRel = Math.Round((double)(((double)(dp.PostIndex)) / dp.Posts.Count) * 100, 1);
                            _db.Downloads.Update(download);
                            await _db.SaveChangesAsync();
                        }
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// Decides which helper method to call based on the domain of the post provided. *All hepler function have the same set of arguments.
        /// </summary>
        /// <param name="post">Post to download.</param>
        /// <param name="path">Path at which the post should be saved at.</param>
        /// <param name="id">Id of the download.</param>
        /// <returns></returns>
        private async Task SavePost(Post post, string path, int id) {
            switch (post.Domain)
            {
                //Images
                case "i.redd.it":
                    await SaveImage(post, path, id);
                    break;
                case "i.imgur.com":
                    await SaveImage(post, path, id);
                    break;
                case "reddit.com":
                    await SaveMultipleImages(post, path, id);
                    break;
                //Videos
                case "v.redd.it":
                    await SaveVideo(post, path, id);
                    break;
                case "gfycat.com":
                    await SaveVideo(post, path, id);
                    break;
                //txt files
                case "link":
                    SaveLinkPost(post, path, id);
                    break;
                case "comment":
                    SaveComment(post, path, id);   
                    break;
                case "text":
                    SaveTextPost(post, path, id);
                    break;
                //default makes ERROR txt file
                default:
                    SaveErrorPost(post, path, id);
                    break;
            }
        }
        private async Task SaveVideo(Post post,string path, int id) {
            string url = post.Urls[0];
            if (url.Contains('?')) {
                url = url.Substring(0, url.IndexOf('?'));
            }

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
                try //Ignoring errors concerning StopAndDeleteFunction
                {
                    using (var fileStream = File.Create(path + Path.GetExtension(url)))
                    {
                        streamVideoCombined.CopyTo(fileStream);
                    }
                }
                catch (Exception){}
            }
            else {
                Stream streamVideo = await client.GetStreamAsync(url);
                try //Ignoring errors concerning StopAndDeleteFunction
                {
                    using (var fileStream = File.Create(path + Path.GetExtension(url)))
                    {
                        streamVideo.CopyTo(fileStream);
                    }
                }
                catch (Exception) { }
            }
        }
        private async Task SaveImage(Post post, string path, int id) {
            Stream stream = await client.GetStreamAsync(post.Urls[0]);
            using (var fileStream = File.Create(path + Path.GetExtension(post.Urls[0])))
            {
                stream.CopyTo(fileStream);
            }
        }
        private async Task SaveErrorPost(Post post, string path, int id)
        {
            string name = StripName(post.Title);
            using (StreamWriter sw = File.CreateText(path + "\\ERROR_DOMAIN_UNRECOGNIZED_" + name + ".txt"))
            {
                sw.WriteLine("Title: " + post.Title);
                sw.WriteLine("Subreddit: " + post.Subreddit);
                sw.WriteLine("PermaLink: www.reddit.com" + post.PermaLink);
            }
        }
        private async Task SaveMultipleImages(Post post, string path, int id)
        {
            for (int i = 0; i < post.Urls.Count; i++)
            {
                Stream stream = await client.GetStreamAsync(post.Urls[i]);
                using (var fileStream = File.Create(path + "[" + i + "]" + Path.GetExtension(post.Urls[i])))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }
        private void SaveTextPost(Post post, string path, int id) 
        {
            using (StreamWriter sw = File.CreateText(path + ".txt"))
            {
                sw.WriteLine("Title: " + post.Title);
                sw.WriteLine("Subreddit: " + post.Subreddit);
                sw.WriteLine("PermaLink: www.reddit.com" + post.PermaLink);
                sw.WriteLine("Text: \n" + post.SelfText);
            }
        }
        private void SaveComment(Post post, string path, int id)
        {
            using (StreamWriter sw = File.CreateText(path + ".txt"))
            {
                sw.WriteLine("Comment: " + post.Title);
                sw.WriteLine("Subreddit: " + post.Subreddit);
                sw.WriteLine("PermaLink to a comment: www.reddit.com" + post.PermaLink);
            }
        }
        private void SaveLinkPost(Post post, string path, int id)
        {
            using (StreamWriter sw = File.CreateText(path + ".txt"))
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
        private async Task DebugPost(Post post, string path, int id) {
            using (StreamWriter sw = File.CreateText(path + ".txt"));
        }

        /// <summary>
        /// Method for generating the name based on parameters in DownlodProccess.DownlaodParameters.
        /// </summary>
        /// <param name="post">Post for which to generate a Name.</param>
        /// <param name="dp">DownloadProcess in which tis happens.</param>
        /// <param name="indexInFolder">Index to number the files if required.</param>
        /// <returns></returns>
        private string generateName(Post post,DownloadProcess dp, int indexInFolder)
        {
            StringBuilder sb = new StringBuilder();
            DownloadParameters dParams = dp.DownloadParameters;
            if (dParams.Numbering == Entities.Enums.Numbering.Ids) {
                sb.Append(post.Id + "_");
            } else if (dParams.Numbering == Entities.Enums.Numbering.Standard) {
                sb.Append(indexInFolder + "_");
            }
            if (dParams.SubredditName || dParams.DomainName) {
                if (dParams.SubredditName) {
                    if (dParams.DomainName) {
                        //both names
                        if (dParams.NamePriorityIsSubreddit) {
                            //subreddit priority
                            sb.Append(post.Subreddit + "_" + post.Domain + "_");
                        }
                        else {
                            //domain priority
                            sb.Append(post.Domain + "_" + post.Subreddit + "_");
                        }
                    }
                    else {
                        //subreddit name
                        sb.Append(post.Subreddit + "_");
                    }
                }
                else {
                    //domain name
                    sb.Append(post.Domain + "_");
                }
            }
            if (dParams.Title > 0) {
                if (post.Title.Length > dParams.Title)
                {
                    sb.Append(post.Title.Substring(0,dParams.Title));
                }
                else {
                    sb.Append(post.Title);
                }
            }
            if (sb.ToString() == "") { 
                sb.Append(dp.PostIndex);
            }
            if (sb.ToString().EndsWith("_")) {
                sb.Remove(sb.Length-1, 1);
            }
            if (sb.ToString().EndsWith("."))
            {
                sb.Remove(sb.Length-1, 1);
            }
            if (dp.ExistingNames.Contains(sb.ToString()))
            {
                int i = 1;
                sb.Append('_' + i++);
                while (dp.ExistingNames.Contains(sb.ToString()))
                {
                    sb.Remove(sb.Length - 2, 1);
                    sb.Append('_' + i++);
                }
            }
            dp.ExistingNames.Add(sb.ToString());
            return StripName(sb.ToString());
        }

        /// <summary>
        /// Removes any windows forbidden symbols in fileName path 
        /// </summary>
        /// <param name="name">Name to be stripped.</param>
        /// <returns>Stripped string</returns>
        //TODO more platforms check
        private String StripName(String name)
        {
            if (name.Length > 128) {
                name = name.Substring(0,125) + "...";
            }
            return string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
        }
        /// <summary>
        /// Remove downlod proccess and its files.
        /// </summary>
        /// <param name="id">Id of deleted download.</param>
        /// <returns>Returns true if the removal had suceeded.</returns>
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

        /// <summary>
        /// Stops running download and deletes its files.
        /// </summary>
        /// <param name="id">Id of deleted download.</param>
        /// <returns></returns>
        public async Task StopAndRemoveDownloadProcess(int id) {

            DownloadProcess dp = processes.FirstOrDefault(e => e.DownloadId == id);
            dp.TokenSource.Cancel();
            dp.TokenSource.Dispose();
            Thread.Sleep(100);
            _db.Downloads.Remove(_db.Downloads.FirstOrDefault(e => e.Id == id));
            _db.downloadHistories.Remove(dp.DownloadHistory);
            await _db.SaveChangesAsync();
            processes.Remove(dp);
            Thread thread = new Thread(() => {
                DeleteDownloadFiles(id);
            });
            thread.Start();
        }
        /// <summary>
        /// This method tries to remove downlaod directory with all its files. After each of 10 tries it waits for 2 seconds to try again.
        /// </summary>
        /// <param name="id">Id of deleted download.</param>
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
