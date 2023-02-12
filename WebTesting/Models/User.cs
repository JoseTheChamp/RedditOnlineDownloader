using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebTesting.Models
{
	public class User
	{
		[Key]
		public int Id { get; set; }
		public String RedditId { get; set; }
		[Required]
		[Display(Name = "Username")]
		public String UserName { get; set; }
		[Required]
		public String AccessToken { get; set; }
		public String RefreshToken { get; set; }
		[Required]
		public DateTime Registered { get; set; }
		[Required]
		public DateTime LastLogin { get; set; }

		public User(int id, string redditId, string userName, string accessToken, string refreshToken, DateTime registered, DateTime lastLogin)
		{
			Id = id;
			RedditId = redditId;
			UserName = userName;
			AccessToken = accessToken;
			RefreshToken = refreshToken;
			Registered = registered;
			LastLogin = lastLogin;
		}
	}
}
