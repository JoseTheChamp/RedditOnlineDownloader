using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTesting.Models
{
    public class Download
    {
        [Key]
        public int Id { get; set; }
        public int ProgressAbs { get; set; }
        [Required]
        public int ProgressAbsMax { get; set; }
        public double ProgressRel { get; set; }
        [Required]
        public DateTime DownloadStart { get; set; }
        public DateTime DownloadFinished { get; set; }
        public string Lenght { get; set; }
        public bool IsFinished { get; set; }
        [Required]
        public User User { get; set; }

        public Download()
        {
        }

        public Download(int id, int progressAbsMax, DateTime downloadStart, User user)
        {
            Id = id;
            ProgressAbsMax = progressAbsMax;
            DownloadStart = downloadStart;
            User = user;
        }
    }
}
