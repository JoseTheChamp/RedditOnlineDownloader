using WebTesting.Entities.Enums;

namespace WebTesting.Entities
{
    public class DownloadParameters
    {
        public Numbering Numbering { get; set; }
        public bool SubredditName { get; set; }
        public bool DomainName { get; set; }
        public bool NamePriorityIsSubreddit { get; set; }
        public int Title { get; set; }
        public bool SubredditFolder { get; set; }
        public bool DomainFolder { get; set; }
        public bool FolderPriorityIsSubreddit { get; set; }
        public bool Empty { get; set; }
        public bool Split { get; set; }

        public DownloadParameters(
            Numbering numbering,
            bool subredditName,
            bool domainName,
            bool namePriorityIsSubreddit,
            int title,
            bool subredditFolder,
            bool domainFolder,
            bool folderPriorityIsSubreddit,
            bool empty,
            bool split)
        {
            Numbering = numbering;
            SubredditName = subredditName;
            DomainName = domainName;
            NamePriorityIsSubreddit = namePriorityIsSubreddit;
            Title = title;
            SubredditFolder = subredditFolder;
            DomainFolder = domainFolder;
            FolderPriorityIsSubreddit = folderPriorityIsSubreddit;
            Empty = empty;
            Split = split;
        }
    }
}
