using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace jFunc.Jint
{
    internal class JsFiles
    {

        Dictionary<string, byte[]> content = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        JsTransport transport = null;

        internal string Origin { get; private set; }
        internal string Entry { get => transport != null ? transport.Current : ""; }


        internal JsFiles(string url, string password = "")
        {
            transport = new JsTransport(url);                                                                                           // Get transport
            Origin = transport.Uri;                                                                                                     // This is ORIGIN
            var isZip = transport.Current.ToLower().EndsWith(".zip");                                                                   // Is ZIP ?
            var isProtected = transport.Current.ToLower().EndsWith(".protected");                                                       // Is Protected ?
            if (!isZip && !isProtected) return;
            var stream = transport.Get<Stream>(transport.Current);                                                                      // Gets a stream from current
            if (isProtected) stream = new MemoryStream(Crypt.Decrypt(password, stream));                                                // If protected upgrade stream
            using (var data = stream)                                                                                                   // Unzip the data stream    
                foreach (var zipItem in Utils.Unzip(data)) content.Add(zipItem.Key, zipItem.Value);
            transport = null;                                                                                                           // At this moment we do not need transport anymore
        }

        internal string Fetch(string value)
        {
            if (transport == null)
            {
                if (content.Keys.Contains(value)) return Encoding.UTF8.GetString(content[value]);
                throw new Exception("File not found Bundle:" + value);
            }
            return transport.Get<string>(value);
        }

        internal byte[] FetchBin(string value)
        {
            if (transport == null)
            {
                if (content.Keys.Contains(value)) return content[value];
                throw new Exception("File not found Bundle:" + value);
            }
            return transport.Get<byte[]>(value);
        }
    }
}



    