using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;

namespace WebTesting.Services
{
    public static class ResponseConvertor
    {
        public static (string id, string username) ProfileToIdName(string json) {
            dynamic jsonData = JObject.Parse(json);
            return (jsonData.id, jsonData.name);
        }
        public static (string title, string type) PostToTitleType(string json) {
            dynamic jsonData = JObject.Parse(json);
            string domain = jsonData.data.children[0].data.domain;
            string type;
            switch (domain)
            {
                case "i.redd.it":
                    type = "Reddit image.";
                    break;
                case "v.redd.it":
                    type = "Reddit video.";
                    break;
                case "i.imgur.com":
                    type = "Imgur image.";
                    break;
                default:
                    type = "Reddit text post.";
                    break;
            }
            string title = jsonData.data.children[0].data.title;
            return (title, type);
        }
    }
}
