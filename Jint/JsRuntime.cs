using System;
using System.Collections.Generic;
using System.Reflection;
using Jint;
using Jint.Runtime.Interop;
using Jint.Native;
using System.Linq;

namespace jFunc.Js
{
    internal class JsRuntime
    {
        Engine engine;
        HashSet<Type> RegisteredTypes = new HashSet<Type>();
        JsDll dll;

        public object Result { get; set; }

        public Func<string,string> OnFile { get; set; } =(s)=>throw new NotImplementedException();
        public Func<string, byte[]> OnFileBin { get; set; } = (s) => throw new NotImplementedException();
        byte[] SafeOnFileBin(string name) => Utils.TryCatch<byte[]>(() => OnFileBin(name));                                                         // Safe version of OnFileBin...

        public JsRuntime()
        {
            engine = new Engine(cfg => cfg.AllowClr());                                                                                             // Creates an Engine with CLR enabled
            dll = new JsDll(engine);
              
            engine.SetValue("alphanum", new Func<string, string>((a) => Utils.AlphaNum(a)));
            engine.SetValue("unique", new Func<string>(() => Utils.Unique()));
            engine.SetValue("sleep", new Action<int>((k) => System.Threading.Thread.Sleep(k)));
            engine.SetValue("require", new Func<string, object>(Require));
            engine.SetValue("load", new Func<string, string>( (s)=> OnFile(s)));
            engine.SetValue("loadBin", new Func<string, byte[]>((s) => OnFileBin(s)));
            engine.SetValue("register", new Func<string,int>( (name) => dll.Register(OnFileBin(name))));
            engine.SetValue("AkamaiApi", TypeReference.CreateTypeReference(engine, typeof(jFunc.Akamai.AkamaiApi)));
            engine.SetValue("RestApi", TypeReference.CreateTypeReference(engine, typeof(jFunc.Rest.RestApi)));
            engine.SetValue("OpenSSL", TypeReference.CreateTypeReference(engine, typeof(jFunc.OpenSSL)));
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
            var requireScript = "(function(){\n" + Utils.Indent(content)+ "\n\n})();";                                                          // Wrap the required script. We don't pollute the global namespace  
            JsValue result=engine.Evaluate(requireScript);
            return result;
        }


        Assembly Resolve(AppDomain ad ,string name)
        {
            var assembly = ad.GetAssemblies().FirstOrDefault(a => a.FullName == name);                                                          // Find if already loaded
            if (assembly != null) return assembly;                                                                                              // If so then return it
            var filename= name.Split(',')[0].Trim();                                                                                            // In case we have a full name...split it
            if (!filename.ToLower().EndsWith(".dll")) filename = filename + ".dll";                                                             // Make sure it ends with DLL
            var b = SafeOnFileBin(filename);                                                                                                    // Safe load (ie: doesn´t throw on error)
            if (b != null)  return Assembly.Load(b);                                                                                            // If loaded then return the assembler
            return null;                                                                                                                        // Null...so it goes alongside the pipeline
        }

        public bool Execute(string script)
        {
            try
            {
                var runScript = "(function(){ " + script + "})();";                                                                             // Wrap the script (so that we don´t pollute global)
                AppDomain ad = AppDomain.CurrentDomain;                                                                                         // This is current domain
                ResolveEventHandler handler = (sender, args) => Resolve(ad,args.Name);                                                          // Resolve for needed assemblies
                ad.AssemblyResolve += handler;                                                                                                  // Hook assembly resolve
                Result = engine.Evaluate(runScript).ToObject();                                                                                 // Run
                ad.AssemblyResolve -= handler;                                                                                                  // Remove handler    
                return true;                                                                                                                    // Return true
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
