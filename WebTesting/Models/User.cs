using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTesting.Models
{
	public class User
	{
		[Key]
		public int Id { get; set; }
		[Required]
		[ForeignKey("RedditIdFK")]
		[MaxLength(10)]
		public String RedditId { get; set; }
		[Required]
		[Display(Name = "Username")]
		public String UserName { get; set; }
		[Required]
		public String AccessToken { get; set; }
		[Required]
		public DateTime Registered { get; set; }
		[Required]
		public DateTime LastLogin { get; set; }

		public User(int id, string redditId, string userName, string accessToken, DateTime registered, DateTime lastLogin)
		{
			Id = id;
			RedditId = redditId;
			UserName = userName;
			AccessToken = accessToken;
			Registered = registered;
			LastLogin = lastLogin;
		}
	}
}
