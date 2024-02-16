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
            var version = System.IO.File.GetLastWriteTime(location).ToString("yyyy.MM.dd.HH.mm.ss");


            return new OkObjectResult(new { version = version });
        }

        [FunctionName("identity")]
        public static IActionResult Identity([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log)                                                 
        {
            return new OkObjectResult(new { name= "This is identity"});
        }

    }
}
