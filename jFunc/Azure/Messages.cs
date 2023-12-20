using System;
using System.Net.Http;
using System.Xml;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace jFunc.Azure
{
    public class Messages
    {
        internal string baseUrl;
        internal string sas;
        internal const char delimiter = '/';                                                                                                        // Defined by uploader !!!
        internal const string MSAPI = "2022-11-02";                                                                                                 // Api version                             
        internal string error = "";

        public Messages(string wUrl,string wSas) 
        {
            baseUrl = wUrl;
            sas = wSas;
        }

        (HttpContent Content,int Size) CreateContent(object msg)
        {
            if (msg == null) return (null,0);
            if (msg is string)
            {
                var s = (string)msg;
                return (new StringContent(s, Encoding.UTF8, "text/plain"), s.Length);
            }
            if (msg is byte[])
            {
                var b = (byte[])msg;
                return (new ByteArrayContent(b),b.Length);
            }
            throw new Exception("Unkown CONTENT type "+msg.GetType());
        }

        public HttpRequestMessage CreateMessage(HttpMethod method,string url,object msg = null)
        {
            var httpRequestMessage = new HttpRequestMessage()
            {
                Method = method,
                RequestUri = new System.Uri(url)
            };
            httpRequestMessage.Headers.Add("x-ms-version", MSAPI);
            httpRequestMessage.Headers.Add("date-length", DateTime.Now.ToString("r"));
            var c= CreateContent(msg);
            if (c.Content!=null)
            {
                httpRequestMessage.Content = c.Content;
                httpRequestMessage.Content.Headers.Add("Content-length", c.Size.ToString());

            }
            return httpRequestMessage;
        }

        public string GetUrl(string path) => baseUrl + "/" + path + "?" + sas;

        public bool BlobExists(string path)                                                                                         // Return Yes or False if a BLOB exists or not
        {
            error = "";
            var tUrl = baseUrl + "/" + path + "?" + sas;
            using (var client = new HttpClient())
            {
                var resp = client.SendAsync(CreateMessage(HttpMethod.Head, tUrl)).Result;
                //string body = resp.Content.ReadAsStringAsync().Result;
                return resp.StatusCode != System.Net.HttpStatusCode.NotFound;
            }
        }

        bool CheckError(HttpResponseMessage resp)
        {
            if (resp.IsSuccessStatusCode)
            {
                error = "";
                return true;
            }
            error= resp.Content.ReadAsStringAsync().Result;
            return false;
        }

        public bool CreateBlob(string path)                                                                                         // Return Yes or False if a BLOB exists or not
        {
            var tUrl = baseUrl + "/" + path + "?" + sas;
            using (var client = new HttpClient())
            {
                var msg=CreateMessage(HttpMethod.Put, tUrl,"");
                msg.Headers.Add("x-ms-blob-type", "AppendBlob");

                // TRY THIS FOR THE CONTENT
                // Content-Type                 https://learn.microsoft.com/en-us/rest/api/storageservices/put-blob?tabs=azure-ad
                // or x-ms-blob-content-type
                return CheckError(client.SendAsync(msg).Result);
            }
        }

        public bool Append(string path,string line)                                                                                         // Return Yes or False if a BLOB exists or not
        {
            var tUrl = baseUrl + "/" + path+ "?" + sas + "&comp=appendblock";
            using (var client = new HttpClient())
            {
                var msg = CreateMessage(HttpMethod.Put, tUrl, line);
                msg.Headers.Add("x-ms-blob-type", "AppendBlob");
                return CheckError(client.SendAsync(msg).Result);
            }
        }

        public bool Append(string path, byte[] data)                                                                                         // Return Yes or False if a BLOB exists or not
        {
            var tUrl = baseUrl + "/" + path + "?" + sas + "&comp=appendblock";
            using (var client = new HttpClient())
            {
                var msg = CreateMessage(HttpMethod.Put, tUrl, data);
                msg.Headers.Add("x-ms-blob-type", "AppendBlob");
                return CheckError(client.SendAsync(msg).Result);
            }
        }


        public bool Delete(Blob blob)
        {
            var tUrl = baseUrl + delimiter + (blob.Path != "" ? blob.Path + delimiter : "") + blob.Name + "?" + sas + "&delimiter=" + delimiter;
            using (var client = new HttpClient())
            {
                var msg = CreateMessage(HttpMethod.Delete, tUrl);
                return CheckError(client.SendAsync(msg).Result);
            }
        }

        public bool Delete(string bUrl)
        {
            var tUrl = baseUrl + delimiter + bUrl + "?" + sas + "&delimiter=" + delimiter;
            using (var client = new HttpClient())
            {
                var msg = CreateMessage(HttpMethod.Delete, tUrl);
                return CheckError(client.SendAsync(msg).Result);
            }
        }


        public Blob Get(string path)
        {
            error = "";
            var tUrl = baseUrl + delimiter + path + "?" + sas + "&delimiter=" + delimiter;
            using (var client = new HttpClient())
            {
                var resp = client.SendAsync(CreateMessage(HttpMethod.Head, tUrl)).Result;
                string body = resp.Content.ReadAsStringAsync().Result;
                int size = 0;
                foreach (var x in resp.Content.Headers) if (x.Key.ToLower()== "content-length" && x.Value.Count()>0) Int32.TryParse(x.Value.First(), out size);
                return (size > 0 ? new Blob(this, path, size):null);
            }

        }


        public Blob[] List(string path)
        {
            error = "";
            var tUrl = baseUrl + "?restype=container&comp=list&include=metadata&" + sas + "&delimiter=" + delimiter;
            if (path!= "") tUrl = tUrl + "&prefix=" + path + delimiter;                                                     // If a folder has been received then use folder as prefix
            using (var client = new HttpClient())
            {
                var resp = client.SendAsync(CreateMessage(HttpMethod.Get, tUrl)).Result;
                XmlDocument doc = new XmlDocument();
                string body = resp.Content.ReadAsStringAsync().Result;
                doc.LoadXml(body);
                //Console.WriteLine(body);
                var entries = doc.DocumentElement["Blobs"].ChildNodes.Cast<XmlElement>().Select(x => new Blob(this, x));
                return entries.OrderBy(x => x.Name).ToArray();
            }

        }

        public bool Download(Blob blob, string folder)
        {
            folder = folder.Replace('\\', '/');
            if (!folder.EndsWith("/")) folder = folder + "/";
            return Download(blob.Path + "/" + blob.Name, folder + blob.Name);
        }

        public bool Download(string path,string target)
        {
            try
            {
                error = "";
                var tUrl = baseUrl + delimiter + path + "?" + sas + "&delimiter=" + delimiter;
                using (var client = new HttpClient())
                {
                    var resp = client.SendAsync(CreateMessage(HttpMethod.Get, tUrl)).Result;
                    var st = resp.Content.ReadAsStreamAsync().Result;
                    using (var fs = new FileStream(target, FileMode.Create)) st.CopyTo(fs);
                    if (!System.IO.File.Exists(target)) throw new Exception("Could not create " + target);
                    long size = 0;
                    foreach (var x in resp.Content.Headers) if (x.Key.ToLower() == "content-length" && x.Value.Count() > 0) long.TryParse(x.Value.First(), out size);
                    var dsize = (new FileInfo(target)).Length;
                    if (dsize != size) throw new Exception("Size of downloaded file does not match: " + target);
                    return true;
                }
            }
            catch (Exception e)
            {
                error = "ERROR: Downloading " + path + ". " + e.Message;
                return false;
            }
        }
        public string Fetch(string path)
        {
            try
            {
                error = "";
                var tUrl = baseUrl + delimiter + path + "?" + sas + "&delimiter=" + delimiter;
                using (var client = new HttpClient())
                {
                    var resp = client.SendAsync(CreateMessage(HttpMethod.Get, tUrl)).Result;
                    var st = resp.Content.ReadAsStreamAsync().Result;
                    using (StreamReader reader = new StreamReader(st, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                error = "ERROR: Downloading " + path + ". " + e.Message;
                return "";
            }
        }
    }
}
