using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Web;

namespace WebTesting.Pages
{
	public class LoggedInModel : PageModel
	{
		public string Code { get; set; }
		public string Token { get; set; }
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

				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(auth));
				var response = await client.PostAsync("https://www.reddit.com/api/v1/access_token", new FormUrlEncodedContent(data));
				var contents = await response.Content.ReadAsStringAsync();

				//getting token from string response
				{
					dynamic Jsondata = JObject.Parse(contents);
					Token = Jsondata.access_token;
				}

				HttpContext.Session.SetString("token",Token);



				/*
				
				var auth = Encoding.ASCII.GetBytes($"9RevD-RRlRmNcGc3nsu-pg:N_yUDCCT3l_FrTbXVkF_Jgj8Y3_aLg");
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(auth));
				//client.DefaultRequestHeaders.Append("Content-Type:application/x-www-form-urlencoded");
				//client.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");
				HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, "http://www.reddit.com/api/v1/access_token");
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
				req.Content = new StringContent("{\"grant_type\":\"authorization_code\",\"code\":\"" + Code + "\",\"redirect_uri\":\"https://localhost:44335/LoggedIn}",Encoding.UTF8, "application/x-www-form-urlencoded");

				var response = client.SendAsync(req);

				//var response = await client.PostAsync("http://www.reddit.com/api/v1/access_token", new FormUrlEncodedContent(data));
				//var contents = await response.Content.ReadAsStringAsync();
				
				//getting token from string response
				/*
				{
					dynamic Jsondata = JObject.Parse(contents);
					Token = Jsondata.access_token;
				}
				*/
			}
		}
	}
}
