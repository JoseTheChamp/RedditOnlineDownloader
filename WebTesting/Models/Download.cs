using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

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
        [AllowNull]
        public DateTime? DownloadFinished { get; set; }
        public bool IsFinished { get; set; }
        public bool IsDownloadable { get; set; }


        [Required]
        public User User { get; set; }

        public Download()
        {
        }

        public Download(int id, int progressAbsMax, DateTime downloadStart, User user)
        {
            Id = id;
            ProgressAbs = 0;
            ProgressAbsMax = progressAbsMax;
            ProgressRel = 0;
            DownloadStart = downloadStart;
            DownloadFinished = null;
            IsFinished = false;
            User = user;
            IsDownloadable = false;
        }
    }
}
