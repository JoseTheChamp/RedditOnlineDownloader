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
    }
}
