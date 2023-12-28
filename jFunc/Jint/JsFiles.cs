using JFunc.Utils;
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
    internal class JsFiles
    {

        Dictionary<string, byte[]> content = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        internal string Origin { get; private set; }
        internal string Name { get; private set; }
        internal string Path { get; private set; }
        internal string Query { get; private set; }


        internal string urlBase = "";

        internal string Fetch(string value)
        {
            if (Name!="")   return (Path + value + (Query != "" ? Query : "")).HttpGet<string>();
            if (content.Keys.Contains(value)) return Encoding.UTF8.GetString(content[value]);
            throw new Exception("File not found Bundle:" + value);
        }

        internal byte[] FetchBin(string value)
        {
            if (Name != "") return (Path + value + (Query != "" ? Query : "")).HttpGet<byte[]>();
            if (content.Keys.Contains(value)) return content[value];
            throw new Exception("File not found:" + value);
        }

        Stream GetStream(string name,string url,string password)
        {
            if (name.EndsWith(".zip")) return url.HttpGet<Stream>();
            if (name.EndsWith(".protected")) return new MemoryStream(Crypt.Decrypt(password, url.HttpGet<Stream>()));
            throw new Exception("Unknown bundle");
        }


        internal JsFiles(string url,string password="")
        {
            Origin= url;
            var uri = new Uri(url);                                                                                                                             // This is the URL
            Query = uri.Query;                                                                                                                                  // This is the QUERY 
            var items = uri.GetLeftPart(UriPartial.Path).Split('/');                                                                                            // Let's split the URL path
            Name = items.Last();                                                                                                                                // Get code start point
            Path = String.Join("/", items.SkipLast(1)) + "/";                                                                                                   // Get path
            if (Name.ToLower().EndsWith(".js")) return;                                                                                                         // IF name ends with .js then we are done

            using (var data=GetStream(Name.ToLower(),url,password))                                                                                             // Unzip the data stream    
                foreach (var zipItem in Utils.Unzip(data)) content.Add(zipItem.Key, zipItem.Value);

            Name = "";                                                                                                                                          // If we got here, then we are using a bundle and we don't need name or query
            Query = "";
        }
    }
}
