using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace WebTesting.Models
{
    public class Statistic
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(10)]
        public string UserId { get; set; } //SYSTEM - for system statistics
        public int Downloads { get; set; }
        public int DownloadedPosts { get; set; }
        public string DomainsJson { get; set; }
        public string SubredditsJson { get; set; }

        public Statistic(int id, string userId)
        {
            Id = id;
            UserId = userId;
            Downloads = 0;
            DownloadedPosts = 0;
            DomainsJson = string.Empty;
            SubredditsJson = string.Empty;
        }
    }
}
