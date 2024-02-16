using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using jFunc.Js;

namespace jFunc.Jint
{
    public class JsHost
    {
        JsFiles files;
        JsRuntime js = new JsRuntime();
        List<string> scriptLog = new List<string>();                                                                                                                                 // Script log    
        
        public JsRuntime Runtime { get => js; }

        public object LastResult { get => js.Result; }
        public string LastOrigin { get; private set; } = "";

        public DateTime LastStart  { get; private set; } = DateTime.MinValue;
        public double LastDurationMs { get; private set; } = 0;
        public string[] LastLog { get; private set; } = new string[0];
            

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
            js.OnFile = (fname) => files.Fetch(fname) ;                                                                                                               // OnFile 
            js.OnFileBin = (fname) => files.FetchBin(fname);                                                                                                          // OnFile 
            js.Define("log", (Action<object>)Log);                                                                                                                    // Define log in the script
        }

        public void Define(string name, Delegate f) => js.Define(name, f);                                                                                            //            js.Define("query", (Func<string, string>)(s => s.Trim().StartsWith("_") ? "" : req.Query.Get(s)));


        public bool Run(string start)                                                                                                // Run the main        
        {
            LastStart = DateTime.Now;
            LastOrigin = files.Origin;
            start = files.Entry != "" ? files.Entry : (start == "" ? "main.js" : start);                                                                                        // If we have a name from files, if not the received, if not main.js
            var ok=js.Execute(files.Fetch(start));                                                                                                                  // Execute
            LastDurationMs = (DateTime.Now - LastStart).TotalMilliseconds;
            LastLog = scriptLog.ToArray();
            return ok;
        }


        public IActionResult Run(string start, bool nowrap, bool nodata,object pars=null)                                                                                            // Run the main        
        {
            var ok= Run(start);
            if (js.Result ==null) return new BadRequestObjectResult("JS returned no result");                                                                         // We should always get a result, even if the script failed and OK is false
            if (nodata) js.Result = ok;                                                                                                                           // If we don't want result data then use TRUE or FALSE
            if (nowrap) return ok ? new OkObjectResult(js.Result) : new BadRequestObjectResult(js.Result.ToString());                                             // If nowrap then return the result
            dynamic res = new ExpandoObject();                                                                                                                        // Otherwise wrap the result in a JSON object
            res.ok = ok;
            res.name = LastOrigin;
            if (pars != null) res.parameters = pars;
            res.start = LastStart.ToISO8601();
            res.duration = LastDurationMs + " ms";
            res.log = scriptLog.ToArray();
            if (!nodata) res.data = js.Result;                                                                                                                        // If wrapping, include DATA or not
            return new JsonResult(res, new JsonSerializerSettings() { Formatting = Formatting.Indented });
        }


        /*
        public IActionResult Run(string start,bool nowrap,bool nodata)                                                                                                // Run the main        
        {
            start=files.Entry!= "" ? files.Entry: (start==""?"main.js":start);                                                                                        // If we have a name from files, if not the received, if not main.js
            var startTime = DateTime.Now;                                                                                                                             // Starting time
            Console.WriteLine("Executing " + start);
            var ok = js.Execute(files.Fetch(start));                                                                                                                  // Execute
            
            if (js.Result == null) return new BadRequestObjectResult("JS returned no result");                                                                        // We should always get a result, even if the script failed and OK is false
            if (nodata) js.Result = ok;                                                                                                                               // If we don't want result data then use TRUE or FALSE
            if (nowrap) return ok ? new OkObjectResult(js.Result) : new BadRequestObjectResult(js.Result.ToString());                                                 // If nowrap then return the result
            dynamic res = new ExpandoObject();                                                                                                                        // Otherwise wrap the result in a JSON object
            res.ok = ok;
            res.name= files.Origin;
            res.start = startTime.ToISO8601();
            res.duration = (DateTime.Now - startTime).TotalMilliseconds + " ms";
            res.log = scriptLog.ToArray();
            if (!nodata) res.data = js.Result;                                                                                                                        // If wrapping, include DATA or not
            return new JsonResult(res, new JsonSerializerSettings() { Formatting = Formatting.Indented });
        }*/
    }
}
