using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Web;
using WebTesting.Entities;
using WebTesting.Entities.Enums;
using WebTesting.Models;
using WebTesting.Services;
using WebTesting.Utils;

namespace WebTesting.Pages
{
	public class LoggedInModel : PageModel
	{
		public string Code { get; set; }
		public string Token { get; set; }

		private readonly RedditAPI _reddit;
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
				string url = HttpContext.Request.GetEncodedUrl();
				string? code = HttpUtility.ParseQueryString(url).Get("code");
				if (code != null)
				{
					Code = code;
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
					var json = await _reddit.GetProfile(Token);
					var resultIdAndName = ResponseConvertor.ProfileToIdName(json);
					User? user = await _db.Users.FirstOrDefaultAsync(e => e.RedditId.Equals(resultIdAndName.id));
					HttpContext.Session.SetString("AccessToken", Token);
					HttpContext.Session.SetString("UserName", resultIdAndName.username);
					HttpContext.Session.SetString("RedditId", resultIdAndName.id);
					if (user != null)
					{
						user.LastLogin = DateTime.Now;
						user.AccessToken = Token;
						user.UserName = resultIdAndName.username;
						_db.Users.Update(user);
						await _db.SaveChangesAsync();
					}
					else
					{
						await _db.Users.AddAsync(new User(
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
					/*
					HttpContext.Session.SetObject("PostsStatus",PostsResult.Working);
					var sc = SynchronizationContext.Current;
					//HttpContext ctx = this.HttpContext;
					Thread thread = new Thread(async () => {
						List<Post> posts = new List<Post>();
						try
						{
							posts = await _reddit.GetAllSavedPosts(Token, resultIdAndName.username);
						}
						catch (Exception ex)
						{
							HttpContext.Session.SetObject("PostsStatus", PostsResult.RedditApiError);

						}
						if (posts != null)
						{
							HttpContext.Session.SetObject("PostsStatus", PostsResult.Success);
							HttpContext.Session.SetObject("Posts", posts);
						}
					});
					thread.Start();
					thread.Join();
					*/
				}
				else {
					Code = "Failed to login through reddit.";
				}
			}
		}
	}
}
