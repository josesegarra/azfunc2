using jFunc.Akamai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace jFunc.Rest
{
    public class RestApi
    {
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        HttpClient client = new HttpClient();

        public RestApi()
        {
        }


        public static RestResponse GetUrl(string url) => (new RestApi()).Get(url);

        void AddHeaders(HttpRequestMessage request)
        {
            foreach (var header in Headers) request.Headers.Add(header.Key, header.Value);
        }

        public RestResponse Get(string url)
        {
            var uri = new Uri(url);
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                AddHeaders(request);
                var response = client.SendAsync(request).Result;
                return new RestResponse(response);
            }
        }
        public RestResponse Post(string url, string data)
        {
            var uri = new Uri(url);
            ServicePointManager.Expect100Continue = false;
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                data = data.Trim();
                if (data.Length>0) request.Content=new StringContent(data);
                AddHeaders(request);
                var response = client.SendAsync(request).Result;
                return new RestResponse(response);
            }
        }
    }
}
