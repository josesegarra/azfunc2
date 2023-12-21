using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using JFunc.Utils;
using jFunc.Jint;
using Esprima.Ast;
using Newtonsoft.Json;
using System.IO;

namespace jFunc
{
    // C:\projects\explore\2023\2023_11_az_resources
    // ?bundle=http://localhost/az/cb1_day.zip

    public static class HttpApi
    {

        internal static ILogger logger = null;


        internal static void Log(string s)
        {
            if (logger == null) return;
            logger.Log(LogLevel.Critical, s);
        }


        public static void Mock()
        {
            Console.WriteLine("Mocking a request4"); 
        }

        public class TokenRequest
        {
            public string key { get; set; }
            public string value { get; set; }

        }

        [FunctionName("token")]
        public static IActionResult Token([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)              // This function returns a token to access a script. It requires the KEY and the script path in VALUE
        {   
            return Utils.TryCatch<ObjectResult>(
                () =>
                {
                    var content = new StreamReader(req.Body).ReadToEndAsync().Result;
                    TokenRequest r= JsonConvert.DeserializeObject<TokenRequest>(content);
                    return new OkObjectResult(EncryptionHelper.Encrypt(r.key,r.value));
                },
                (e)=> new BadRequestObjectResult("Error in api/token: " + e.FullText())
            );
        }


        // http://localhost:7068/api/HttpExample?_key=this_is_key&_start=http://localhost/az/test.js&cb=cb1&cp=43428&f=DAY
        [FunctionName("run")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,ILogger log)
        {
            try
            {
                logger = log;
                var value=HttpAuthorize.DecryptValue(req.Query.Get("_token"));                                                                          // Get and decrypt TOKEN using system key
                if (!value.ToLower().StartsWith("http:") && !value.ToLower().StartsWith("https:")) throw new Exception("Bad value");                    // Must be an HTTP point        
                var host = new JsHost(value);                                                                                                           // Create a HOST capable of running VALUE
                host.Define("query", (Func<string, string>)(s => s.Trim().StartsWith("_") ? "" : req.Query.Get(s,Utils.IfNotFound.returnEmpty)));       // Define the QUERY function in the host that just returns the query parameter    
                bool nodata = req.Query.Get("_nodata",Utils.IfNotFound.returnEmpty) != "";                                                              // Do we want a result at all....
                bool nowrap = req.Query.Get("_nowrap", Utils.IfNotFound.returnEmpty) != "";                                                             // Are we wrapping the result
                var result =host.Run(nowrap, nodata);
                return result;
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("This error leaked somehow: "+ex.FullText());
            }
        }
    }
}
