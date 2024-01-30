using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using jFunc.Jint;
using System.IO;

namespace jFunc
{
    // C:\projects\explore\2023\2023_11_az_resources
    // ?bundle=http://localhost/az/cb1_day.zip

    public static class HttpApi
    {

        //  To test: 
        //      1. Have in http://localhost/az/main.js the code to RUN
        //      2. Encrypt using a token THE PATH to the code:
        //              curl.exe  -F "token=pepe0124567890123456789" -F "value=http://localhost/az/main.js" http://localhost:7068/api/protect
        // Returns:
        //              y7Kxy1s6dVMo_nnvNZnefVANnws2B6iJKJiOjx35qVY-
        //
        //      3. Execute the code
        //              curl.exe  -F "token=pepe0124567890123456789" "http://localhost:7068/api/run/y7Kxy1s6dVMo_nnvNZnefVANnws2B6iJKJiOjx35qVY-?report=CB4_DAY&_nodata=1"

        [FunctionName("run")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "run/{path}")] HttpRequest req, ILogger log,string path)
        {
            try
            {
                HttpLogger.logger = log;
                var form = req.ReadFormAsync().Result;                                                                                                                                  // Get form data
                var token = form.ContainsKey("token") ? form["token"].ToString().Trim() : throw new Exception("Missing token value in form");                                           // Get token
                var url = Crypt.Decrypt(token, path);                                                                                                                                   // Get the URL
                if (!url.ToLower().StartsWith("http:") && !url.ToLower().StartsWith("https:")) throw new Exception("Bad value");                                                        // Must be an HTTP point        
                var ftoken = form.ContainsKey("ftoken") ? form["value"].ToString().Trim() : token;                                                                                      // Get form token if present. If not use same as URL
                var host = new JsHost(url,ftoken);                                                                                                                                      // Create a HOST capable of running VALUE
                host.Define("query", (Func<string, string>)(s => s.Trim().StartsWith("_") ? "" : req.Query.Get(s, Utils.IfNotFound.returnEmpty)));                                      // Define the QUERY function in the host that just returns the query parameter    
                bool nodata = req.Query.Get("_nodata", Utils.IfNotFound.returnEmpty) != "";                                                                                             // Do we want a result at all....
                bool nowrap = req.Query.Get("_nowrap", Utils.IfNotFound.returnEmpty) != "";                                                                                             // Are we wrapping the result
                string main = req.Query.Get("main", Utils.IfNotFound.returnEmpty);                                                                                                      // Main!!
                var result = host.Run(main, nowrap, nodata);
                return result;
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("This error leaked somehow: " + ex.FullText());
            }
        }


        [FunctionName("protect")]
        public static IActionResult Protect([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)                                                 // This function returns a token to access a script. It requires the KEY and the script path in VALUE
        {
            return CryptDecrypt(req, Crypt.Encrypt, Crypt.Encrypt);
        }

        [FunctionName("unprotect")]
        public static IActionResult UnProtect([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)                                               // This function returns a token to access a script. It requires the KEY and the script path in VALUE
        {
            return CryptDecrypt(req, Crypt.Decrypt, Crypt.Decrypt);
        }


        
        static IActionResult CryptDecrypt(HttpRequest req, Func<string, Stream, byte[]> doFile, Func<string, string, string> doValue)
        {
            try
            {
                var form = req.ReadFormAsync().Result;                                                                                                                                      // Get form data
                var token = form.ContainsKey("token") ? form["token"].ToString().Trim() : throw new Exception("Missing token value in form");                                               // Get token
                if (token.Length < 18) throw new Exception("Token length needs to be at least 18 chars");                                                                                   // Validate token length

                var file = req.Form.Files.GetFile("file");                                                                                                                                  // Check if file
                if (file != null) using (var stream = file.OpenReadStream()) return new FileContentResult(doFile(token, stream), "application/binary") { FileDownloadName = "data" };       // If file, then encrypt or decrypt the file

                if (!form.ContainsKey("value")) throw new Exception("Protect needs a file or a value parameter");                                                                           // If not file, then we need  a value
                return new OkObjectResult(doValue(token, form["value"].ToString().Trim()));                                                                                                 // Encrypt or decrypt the value
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult("Error: " + e.FullText());
            }
        }

    }
}
