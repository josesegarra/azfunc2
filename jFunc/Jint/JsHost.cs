using jFunc.Js;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Security.Principal;

namespace jFunc.Jint
{
    internal class JsHost
    {
        JsFiles files;
        JsRuntime js = new JsRuntime();
        List<string> scriptLog = new List<string>();                                                                                                                                 // Script log    
        string start;
        
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



        public JsHost(string value) {
            var bundle = (new Uri(value)).AbsolutePath.ToLower().EndsWith(".zip") ? value : "";                                                                         // If this ends in .ZIP then this is a BUNDLE
            files = new JsFiles(bundle);                                                                                                                // Files with a BUNDLE or without it
            start = bundle == "" ? value : "main.js";                                                                                                                   // If no BUNDLE, then Value holds the start script, If BUNDLE default to main.js
            js.OnFile = (fname) => files[fname] ;                                                                                                                       // OnFile 
            js.Define("log", (Action<object>)Log);                                                          // Define log in the script
        }

        public void Define(string name, Delegate f) => js.Define(name, f);                                                                                              //            js.Define("query", (Func<string, string>)(s => s.Trim().StartsWith("_") ? "" : req.Query.Get(s)));

        public IActionResult Run(bool nowrap,bool nodata)                                                                                                                           // Run the main        
        {
            var startTime = DateTime.Now;                                                                                                                               // Starting time
            var ok = js.Execute(files[start]);                                                                                                                          // Execute
            if (js.Result == null) return new BadRequestObjectResult("JS returned no result");                                                                          // We should always get a result, even if the script failed and OK is false
            if (nodata) js.Result = ok;                                                                                                                                 // If we don't want result data then use TRUE or FALSE
            if (nowrap) return ok ? new OkObjectResult(js.Result) : new BadRequestObjectResult(js.Result.ToString());                                                   // If nowrap then return the result
            dynamic res = new ExpandoObject();                                                                                                                          // Otherwise wrap the result in a JSON object
            res.ok = ok;
            res.start = startTime.ToISO8601();
            res.duration = (DateTime.Now - startTime).TotalMilliseconds + " ms";
            res.log = scriptLog.ToArray();
            if (!nodata) res.data = js.Result;                                                                                                                        // If wrapping, include DATA or not
            return new JsonResult(res, new JsonSerializerSettings() { Formatting = Formatting.Indented });
        }
    }
}
