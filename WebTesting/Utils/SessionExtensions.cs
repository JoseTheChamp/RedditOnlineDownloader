using Newtonsoft.Json;
using WebTesting.Entities;

namespace WebTesting.Utils
{
	public static class SessionExtensions
	{
		public static void SetObject(this ISession session, string key, object value)
		{
			//var seril = JsonConvert.SerializeObject(value);
			//var des = JsonConvert.DeserializeObject<List<Post>>(seril);
			session.SetString(key, JsonConvert.SerializeObject(value));
		}

		public static T GetObject<T>(this ISession session, string key)
		{
			var value = session.GetString(key);
			return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
		}
	}
}
