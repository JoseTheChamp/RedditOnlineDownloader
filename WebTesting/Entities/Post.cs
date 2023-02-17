using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BakalarBackend
{
    public class Post
    {
        public string Id { get;}
        public string Title { get;}
        public string SelfText { get;}
        public string Subreddit { get;}
        public string Author { get;}
        public string IsRedditMediaDomain { get;}
        public string Domain { get;}
        public string Over18 { get;}
        public string PermaLink { get;}
        public string Created { get;}
        public string Ups { get;}
        public string NumberOfComments { get;}
        public List<string> Urls { get;}

        public Post(string id, string title, string selfText, string subreddit, string author, string isRedditMediaDomain, string domain, string over18, string permaLink, string created, string ups, string numberOfComments, List<string> urls)
        {
            Id = id;
            Title = title;
            SelfText = selfText;
            Subreddit = subreddit;
            Author = author;
            IsRedditMediaDomain = isRedditMediaDomain;
            Domain = domain;
            Over18 = over18;
            PermaLink = permaLink;
            Created = created;
            Ups = ups;
            NumberOfComments = numberOfComments;
            Urls = urls;
        }
    }
}
