using System;
using System.Collections.Generic;
using System.Linq;
using Jint;
using Jint.Runtime.Interop;
using System.IO;
using System.IO.Compression;
using Jint.Native;
using System.Text.Json;
using System.Security.Principal;
using System.Xml.Linq;
using System.Dynamic;
using Newtonsoft.Json;

namespace jFunc.Js
{
    internal class JsRuntime
    {
        Engine engine;
        HashSet<Type> RegisteredTypes = new HashSet<Type>();
        
        public object Result { get; set; }

        public Func<string,string> OnFile { get; set; } =(s)=>throw new NotImplementedException();

        public JsRuntime()
        {
            engine = new Engine(cfg => cfg.AllowClr());                                                                                             // Creates an Engine with CLR enabled


            engine.SetValue("alphanum", new Func<string, string>((a) => Utils.AlphaNum(a)));
            engine.SetValue("unique", new Func<string>(() => Utils.Unique()));
            engine.SetValue("sleep", new Action<int>((k) => System.Threading.Thread.Sleep(k)));
            engine.SetValue("require", new Func<string, object>(Require));
            engine.SetValue("AkamaiApi", TypeReference.CreateTypeReference(engine, typeof(jFunc.Akamai.AkamaiApi)));


            //engine.SetValue("println", new Action<string, string>((a, b) => Utils.PrintLn(a, b == null ? "green" : b)));
            //engine.SetValue("log", new Action<string, string>((a, b) => Utils.AppLog(a, b)));
            //engine.SetValue("info", new Action<string, string>((a, b) => Utils.AppInfo(a, b)));

            /*engine.SetValue("debug", new Action<bool>((s) => debug = s));
            engine.SetValue("warn", new Action<string, string>((a, b) => Utils.AppWarn(a, b)));
            engine.SetValue("info2", new Action<string, string>((a, b) => Utils.AppInfo2(a, b)));
            engine.SetValue("error", new Action<string, string>((a, b) => Utils.AppError(a, b)));
            engine.SetValue("title", new Action<string, string>((a, b) => Utils.AppTitle(a, b)));

            engine.SetValue("oneLine", new Func<string, string>((a) => Utils.OneLine(a)));

            engine.SetValue("toBase64", new Func<byte[], string>((a) => Convert.ToBase64String(a)));
            engine.SetValue("fromBase64", new Func<string, byte[]>((a) => Convert.FromBase64String(a)));
            */
        }

        public void Define(string name,Delegate action) => engine.SetValue(name, action);

        object Require(string file)
        {
            file = file.Trim();
            var content=OnFile(file);
            var requireScript = "(function(){\n" + Utils.Indent(content)+ "\n\n})();";                             // Wrap the required script. We don't pollute the global namespace  
            JsValue result=engine.Evaluate(requireScript);
            return result;
        }


        public bool Execute(string script)
        {
            try
            {
                var runScript = "(function(){ " + script + "})();";
                Result = engine.Evaluate(runScript).ToObject();
                return true;
            }
            catch(Exception s)
            {
                Result = s.FullText();
                return false;
            }
        }
        public void DefineFunction(string name, Delegate func) => engine.SetValue(name, func);

    }



}
