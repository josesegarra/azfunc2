using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using jFunc;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace jFunc.Akamai
{
    public class AkamaiApi
    {
        public const string AuthorizationHeader = "Authorization";

        ClientCredential credential;
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        HttpClient client = new HttpClient();

        

        public AkamaiApi(string clientToken, string accessToken, string secret)
        {
            credential=new ClientCredential(clientToken,accessToken,secret);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var productValue = new ProductInfoHeaderValue("AkamaiAPI", "1.0");
            var commentValue = new ProductInfoHeaderValue("(+http://www.example.com)");
            client.DefaultRequestHeaders.UserAgent.Add(productValue);
            client.DefaultRequestHeaders.UserAgent.Add(commentValue);
            ServicePointManager.EnableDnsRoundRobin = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        string Sign(string method,Uri uri,string data="")
        {
            string timestamp = DateTime.UtcNow.ToISO8601();
            string requestData = AkamaiUtils.GetRequestData(method, uri,data);
            string authData = AkamaiUtils.GetAuthDataValue(credential, timestamp);
            return AkamaiUtils.GetAuthorizationHeaderValue(credential, timestamp, authData, requestData);
        }

        public AkamaiResponse Get(string url)
        {
            var uri = new Uri(url);
            var auth=Sign("GET", uri);
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                request.Headers.Add(AuthorizationHeader,auth);
                var response=client.SendAsync(request).Result;
                return new AkamaiResponse(response);
            }
        }
        public AkamaiResponse Post(string url,string data)
        {
            var uri = new Uri(url);
            var auth = Sign("POST", uri,data);
            ServicePointManager.Expect100Continue = false;
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Headers.Add(AuthorizationHeader, auth);
                request.Content = new StringContent(data,Encoding.UTF8,"application/json");//CONTENT-TYPE header
                var response = client.SendAsync(request).Result;
                return new AkamaiResponse(response);
            }
        }
    }
}



