using System.ComponentModel.DataAnnotations;

namespace WebTesting.Models
{
    public class Template
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Name { get; set; }




        public bool ShowDownloaded { get; set; }
        public bool GroupBySubreddit { get; set; }
        public string Nsfw { get; set; }
        public string DomainsForm { get; set; }


        public string Numbering { get; set; }
        public bool SubredditName { get; set; }
        public bool DomainName { get; set; }
        public bool PriorityName { get; set; }
        public int Title { get; set; }
        public bool SubredditFolder { get; set; }
        public bool DomainFolder { get; set; }
        public bool PriorityFolder { get; set; }
        public bool Empty { get; set; }
        public bool Split { get; set; }

        public override string? ToString()
        {
            return Name;
        }

        public Template(int id, string userId,string name)
        {
            Id = id;
            UserId = userId;
            Name = name;
            ShowDownloaded = false;
            GroupBySubreddit = false;
            DomainName = false;
            PriorityName = true;
            SubredditName= false;
            Nsfw = "both";
            DomainFolder= false;
            SubredditFolder= true;
            Empty = true;
            Split = true;
            PriorityFolder = true;
            Numbering = "ids";
            DomainsForm = "all";
        }

        public Template(int id, 
            string userId, 
            string name,
            bool showDownloaded, 
            bool groupBySubreddit, 
            string nsfw, 
            string domainsForm, 
            string numbering, 
            bool subredditName, 
            bool domainName, 
            bool priorityName, 
            int title, 
            bool subredditFolder, 
            bool domainFolder, 
            bool priorityFolder, 
            bool empty, 
            bool split)
        {
            Id = id;
            UserId = userId;
            Name = name;
            ShowDownloaded = showDownloaded;
            GroupBySubreddit = groupBySubreddit;
            Nsfw = nsfw;
            DomainsForm = domainsForm;
            Numbering = numbering;
            SubredditName = subredditName;
            DomainName = domainName;
            PriorityName = priorityName;
            Title = title;
            SubredditFolder = subredditFolder;
            DomainFolder = domainFolder;
            PriorityFolder = priorityFolder;
            Empty = empty;
            Split = split;
        }
    }
}
