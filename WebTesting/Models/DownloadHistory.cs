using System.ComponentModel.DataAnnotations;

namespace WebTesting.Models
{
    public class DownloadHistory
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(10)]
        public string UserId { get; set; }
        [Required]
        public String DownloadedPosts { get; set; }
        [Required]
        public DateTime DownloadTime { get; set; }

        public DownloadHistory(int id, string userId, string downloadedPosts, DateTime downloadTime)
        {
            Id = id;
            UserId = userId;
            DownloadedPosts = downloadedPosts;
            DownloadTime = downloadTime;
        }
    }
}
