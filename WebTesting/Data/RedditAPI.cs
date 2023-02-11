namespace WebTesting.Data
{
	public class RedditAPI
	{
		public RedditAPI()
		{
			Client = new HttpClient();
			Client.BaseAddress = new Uri("https://www.youtube.com/");
		}
		public HttpClient Client { get; set; }
	}
}
