using Microsoft.AspNetCore.Mvc.Formatters.Internal;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace jFunc.Jint
{
    internal class JsTransport
    {
        string Query;
        string Path;
        string Schema;
        internal string Current { get; private set; }
        internal string Uri {  get => Schema+"://"+Path+Current ; }

        internal JsTransport(string url)
        {
            var uri = new Uri(url);                                                                                                                             // This is the URL
            var items = uri.GetLeftPart(UriPartial.Path).Split('/');                                                                                            // Let's split the URL path
            Current= items.Last();                                                                                                                              // Get code start point
            Query = uri.Query;
            Schema = uri.Scheme;
            if (Schema=="https" || Schema=="http") Path = String.Join("/", items.SkipLast(1)) + "/";                                                            // Get path for HTTP(S)
            if (Schema == "file") Path = String.Join('/', (uri.Host + uri.PathAndQuery).Split('/').SkipLast(1)) + "/";                                          // Get path for file   
        }

        internal T Get<T>(string file)
        {
            var r = Path + file + Query;
            if (Schema=="file") return Utils.FileGet<T>(r);
            if (Schema == "http" || Schema == "https") return Utils.HttpGet<T>(r);
            throw new Exception("Unsupported schema: "+Schema);
        }


    }
}
