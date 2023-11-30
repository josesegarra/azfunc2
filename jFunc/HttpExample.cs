using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using jFunc.Js;
using System.Dynamic;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace jFunc
{
    // C:\projects\explore\2023\2023_11_az_resources
    // ?bundle=http://localhost/az/cb1_day.zip

    public static class HttpExample
    {

        internal static ILogger logger = null;


        internal static void Log(string s)
        {
            if (logger == null) return;
            logger.Log(LogLevel.Critical, s);
        }


        // http://localhost:7068/api/HttpExample?_key=this_is_key&_start=http://localhost/az/test.js&cb=cb1&cp=43428&f=DAY

        [FunctionName("HttpExample")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,ILogger log)
        {
            try
            {
                logger = log;

                var key = Environment.GetEnvironmentVariable("key") ?? "";
                string user_key = req.Query["_key"];
                user_key = (user_key ?? "").Trim();
                if (user_key.Length < 3 || user_key != key) throw new Exception("Missing key parameter");

                var bundle = req.Query.Get("_bundle");
                ScriptFiles files = bundle!="" ? ScriptFiles.FromBundle(bundle):new ScriptFiles();

                var start = req.Query.Get("_start");
                if (bundle != "" && start == "") start = "main.js";

                var js = new JsRuntime() { OnFile = (fname) => files[fname] };

                var scriptLog=new List<string>();
                js.Define("log", (Action<string>)(s => scriptLog.Add("["+DateTime.Now.ToISO8601()+"] "+s)));

                js.Define("query", (Func<string,string>)(s => s.Trim().StartsWith("_") ? "": req.Query.Get(s)));


                var startTime = DateTime.Now;
                var ok = js.Execute(files[start]);
                if (js.Result != null)
                {
                    if (req.Query.Get("_nowrap") != "")
                    {
                        Log("NOWRAP OPTION ["+ req.Query.Get("_nowrap")+"]");
                        Log("NOWRAP OPTION [" + req.Query.Get("_nowrap").Length + "]");

                        return ok ? new OkObjectResult(js.Result) : new BadRequestObjectResult(js.Result.ToString());
                    }
                    dynamic res = new ExpandoObject();
                    res.ok = ok;
                    res.start = startTime.ToISO8601();
                    res.duration = (DateTime.Now - startTime).TotalMilliseconds + " ms";
                    res.log=scriptLog.ToArray();
                    res.data = js.Result;
                    return new JsonResult(res,new JsonSerializerSettings() { Formatting=Formatting.Indented});
                    
                    //return new OkObjectResult(res);
                }
                /*
                string user_key = req.Query["key"];
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                //name = name ?? data?.name;                                                                                                          // Either name came from GET or as 
                user_key = user_key ?? string.Empty;
                var key = Environment.GetEnvironmentVariable("key") ?? string.Empty;

                */
                return new BadRequestObjectResult("Bad request");

            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("Bad Error: "+ex.FullText());
            }
        }
    }
}
