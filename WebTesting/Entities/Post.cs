using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WebTesting.Entities
{
    public class Post
    {
        public string Id { get; }
        public string Title { get; }
        public string SelfText { get; }
        public string Subreddit { get; }
        public string Author { get; }
        public string Domain { get; }
        public bool Over18 { get; }
        public string PermaLink { get; }
        public double Created { get; }
        public int Ups { get; }
        public int NumberOfComments { get; }
        public List<string> Urls { get; }

        public Post(string id, string title, string selfText, string subreddit, string author, string domain, bool over18, string permaLink, double created, int ups, int numberOfComments, List<string> urls)
        {
            Id = id;
            Title = title;
            SelfText = selfText;
            Subreddit = subreddit;
            Author = author;
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
