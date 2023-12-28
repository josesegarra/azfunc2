using Jint;
using System;
using System.Collections.Generic;
using System.Linq;
using Jint.Runtime.Interop;

namespace jFunc.Js
{
    internal class JsDll
    {
        HashSet<Type> rTypes= new HashSet<Type>();
        Engine engine;

        internal JsDll(Engine jsEngine)
        {
            engine = jsEngine;
        }
        int Publish(Type type)
        {
            if (rTypes.Contains(type)) return 0;
            rTypes.Add(type);
            engine.SetValue(type.Name, TypeReference.CreateTypeReference(engine, type));                                                // Create a JS TypeReference for all published types
            return 1;
        }
        internal int Register(byte[] bytes)
        {
            var types = new HashSet<Type>();                                                                                            // This is the list of types that the assembly loads
            AppDomain ad = AppDomain.CurrentDomain;                                                                                     // This is current domain
            var typesBefore = ad.GetAssemblies().SelectMany(a => a.GetTypes()).Where( t=> t.IsPublic);                                  // Get currently loaded types
            var load = ad.Load(bytes);                                                                                                  // Load current assembly
            var newTypes= ad.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => t.IsPublic).Except(typesBefore).ToArray();        // Get currently loaded types
            return newTypes.Select(Publish).Sum();                                                                                      // Publish missing types in Engine    
        }
    }
}
