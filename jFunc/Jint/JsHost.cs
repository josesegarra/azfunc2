using jFunc.Js;
using Jint.Runtime;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Security.Principal;
using System.Xml.Linq;

namespace jFunc.Jint
{
    internal class JsHost
    {
        JsFiles files;
        JsRuntime js = new JsRuntime();
        List<string> scriptLog = new List<string>();                                                                                                                                 // Script log    
        
        void Log(object k)
        {
            var prefix = "[" + DateTime.Now.ToISO8601() + "] ";
            string r = null;
            if (k == null) r = "{null}";
            if (r == null && k is string) r=k.ToString();
            if (r == null && k.GetType().IsPrimitive) r = k.ToString();
            if (r == null) r = JsonConvert.SerializeObject(k, Formatting.Indented).Replace('"', '\'');


            if (r==null) r = k.ToString();

            scriptLog.Add(prefix+r);
        }



        public JsHost(string url,string password)
        {
            files = new JsFiles(url,password);                                                                                                                        // Files with a BUNDLE or without it
            js.OnFile = (fname) => files.Fetch(fname) ;                                                                                                                       // OnFile 
            js.OnFileBin = (fname) => files.FetchBin(fname);                                                                                                                       // OnFile 
            js.Define("log", (Action<object>)Log);                                                          // Define log in the script
        }

        public void Define(string name, Delegate f) => js.Define(name, f);                                                                                              //            js.Define("query", (Func<string, string>)(s => s.Trim().StartsWith("_") ? "" : req.Query.Get(s)));

        public IActionResult Run(string start,bool nowrap,bool nodata)                                                                                                  // Run the main        
        {
            if (files.Name != "") start = files.Name;                                                                                                                   // If we have a name from files, then ignore whatever we received
            if (start == "") start = "main.js";                                                                                                                         // If we have no start, let's use main.js            
            
            var startTime = DateTime.Now;                                                                                                                               // Starting time
            var ok = js.Execute(files.Fetch(start));                                                                                                                          // Execute
            if (js.Result == null) return new BadRequestObjectResult("JS returned no result");                                                                          // We should always get a result, even if the script failed and OK is false
            if (nodata) js.Result = ok;                                                                                                                                 // If we don't want result data then use TRUE or FALSE
            if (nowrap) return ok ? new OkObjectResult(js.Result) : new BadRequestObjectResult(js.Result.ToString());                                                   // If nowrap then return the result
            dynamic res = new ExpandoObject();                                                                                                                          // Otherwise wrap the result in a JSON object
            res.ok = ok;
            res.name= files.Origin;
            res.start = startTime.ToISO8601();
            res.duration = (DateTime.Now - startTime).TotalMilliseconds + " ms";
            res.log = scriptLog.ToArray();
            if (!nodata) res.data = js.Result;                                                                                                                        // If wrapping, include DATA or not
            return new JsonResult(res, new JsonSerializerSettings() { Formatting = Formatting.Indented });
        }
    }
}
