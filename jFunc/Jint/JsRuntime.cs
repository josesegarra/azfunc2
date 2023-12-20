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
            engine.SetValue("RestApi", TypeReference.CreateTypeReference(engine, typeof(jFunc.Rest.RestApi)));
            engine.SetValue("AzureStorage", TypeReference.CreateTypeReference(engine, typeof(jFunc.Azure.Storage)));
            engine.SetValue("yellow", new Action<string>((a) => {
                var c = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(a);
                Console.ForegroundColor = c;
            }));
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
