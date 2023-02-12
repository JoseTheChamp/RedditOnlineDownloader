using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Web;
using WebTesting.Data;
using WebTesting.Models;

namespace WebTesting.Pages
{
	public class LoggedInModel : PageModel
	{
		public string Code { get; set; }
		public string Token { get; set; }

		private RedditAPI _reddit;
		private readonly ApplicationDbContext _db;
		public LoggedInModel(RedditAPI reddit, ApplicationDbContext applicationDbContext)
		{
			_reddit = reddit;
			_db = applicationDbContext;
		}
		public async Task OnGetAsync()
		{
			if (Code == null)
			{
				Code = HttpContext.Request.GetEncodedUrl();
				Code = HttpUtility.ParseQueryString(Code).Get("code");
				//TODO Figure out if successfull if not then something

				HttpClient client = new HttpClient();

				var auth = Encoding.ASCII.GetBytes("9RevD-RRlRmNcGc3nsu-pg:N_yUDCCT3l_FrTbXVkF_Jgj8Y3_aLg");
				
				Dictionary<string, string> data = new Dictionary<string, string>();
				data.Add("grant_type", "authorization_code");
				data.Add("code", Code);
				data.Add("redirect_uri", "https://localhost:44335/LoggedIn");

				//Getting access token
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(auth));
				var response = await client.PostAsync("https://www.reddit.com/api/v1/access_token", new FormUrlEncodedContent(data));
				var contents = await response.Content.ReadAsStringAsync();

				//getting token from string response
				{
					dynamic Jsondata = JObject.Parse(contents);
					Token = Jsondata.access_token;
				}
				var resultIdAndName = await _reddit.GetIdAndNameAsync(Token);
				Models.User? user = await _db.Users.FirstOrDefaultAsync(e => e.RedditId.Equals(resultIdAndName.id)); ;
				if (user != null)
				{
					user.LastLogin = DateTime.Now;
					user.AccessToken = Token;
					user.UserName = resultIdAndName.username;
					_db.Users.Update(user);
					await _db.SaveChangesAsync();
				}
				else {
					_reddit.SetAccessToken(Token);
					HttpContext.Session.SetString("AccessToken", Token);
					HttpContext.Session.SetString("UserName", resultIdAndName.username);

					await _db.Users.AddAsync(new Models.User(
						0,
						resultIdAndName.id,
						resultIdAndName.username,
						Token,
						"",
						DateTime.Now,
						DateTime.Now
						));
					await _db.SaveChangesAsync();
				}
			}
		}
	}
}
