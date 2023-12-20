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

namespace jFunc.Jint
{
    internal class JsFiles
    {

        Dictionary<string, string> content = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        internal string Origin { get; private set; }

        internal string urlBase = "";

        internal string this[string value]
        {
            get => Fetch(value);
        }

        string Fetch(string value)
        {
            var u1 = value.ToLower();
            if (u1.StartsWith("http://") || u1.StartsWith("https://"))                                              // Always fetch http content
            {
                if (urlBase == "") urlBase = Utils.UrlPath(value);
                return value.HttpGet<string>();
            }
            if (urlBase != "") return (urlBase + value).HttpGet<string>();
            if (content.Keys.Contains(value)) return content[value];
            throw new Exception("File not un Bundle:" + value);
        }

        internal JsFiles(string url = "")
        {
            Origin = url;
            if (url == "") return;
            using (var dataStream = url.HttpGet<Stream>())
            {
                using (var zipArchive = new ZipArchive(dataStream, ZipArchiveMode.Read))
                {
                    foreach (var entry in zipArchive.Entries)
                    {
                        using (Stream stream = entry.Open())
                        {
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                stream.CopyTo(memoryStream);
                                memoryStream.Position = 0;
                                var bytes = memoryStream.ToArray();
                                content.Add(entry.Name.Trim(), Encoding.UTF8.GetString(bytes));
                            }
                        }
                    }
                }
            }
        }



    }
}
