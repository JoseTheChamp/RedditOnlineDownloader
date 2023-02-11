using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace WebTesting.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public String Name { get; set; }
        [Display(Name="Display Order")]
        [Range(1,100,ErrorMessage = "Display order must be between values 1 and 100.")]
        public int DisplayOrder { get; set; }
    }
}
