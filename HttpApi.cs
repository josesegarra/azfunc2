using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace jFunc
{
    public static class HttpApi
    {
        [FunctionName("version")]
        public static IActionResult Version([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)                                               
        {
            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var version = "azfunc2 -  "+System.IO.File.GetLastWriteTime(location).ToString("yyyy.MM.dd.HH.mm.ss");


            return new OkObjectResult(new { version = version });
        }

        [FunctionName("identity")]
        public static IActionResult Identity([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log)                                                 
        {
            var n = new Dictionary<string, string>();
            foreach(var header in req.Headers)
            {
                n.Add(header.Key, header.Value.ToString());

            }
            
            
            return new OkObjectResult(new { name= "This is identity" , headers = n});
        }

        [FunctionName("login")]
        public static IActionResult Login([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            var n = new Dictionary<string, string>();
            foreach (var header in req.Headers)
            {
                n.Add(header.Key, header.Value.ToString());

            }


            return new OkObjectResult(new { name = "This is login", headers = n });
        }

    }
}
