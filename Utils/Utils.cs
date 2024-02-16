using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Net.Http;

namespace jFunc
{
    public static class Utils
    {

        public static string FullText(this Exception e)
        {
            var l=new List<string>();
            while (e != null)
            {
                if (!l.Contains(e.Message)) l.Add(e.Message);
                e = e.InnerException;
            }
            return String.Join('\n',l);
        }

        public static string Format(this DateTime d) => d.ToString("yyyy-MM-dd HH:mm:ss");

        public static string Indent(this string str, int size = 4)
        {
            var indent = new String(' ', size);
            String[] strlist = str.Split('\n');
            for (var i = 0; i < strlist.Length; i++) strlist[i] = indent + strlist[i];
            return String.Join("\n", strlist);
        }

        public static string Join(this IEnumerable<string> k, string sep = " | ") => string.Join(sep, k);

        public static string FileToBase64(string path)
        {
            Byte[] bytes = File.ReadAllBytes(path);
            return Convert.ToBase64String(bytes);
        }

        public static T Get<S, T>(this Dictionary<S, T> dict, S key)
        {
            return (dict.TryGetValue(key, out T f) ? f : throw new Exception("Unknown token type [" + key.ToString() + "]"));
        }

        public static string OneLine(string k)
        {
            string r = "";
            int p = 0;
            while (p < k.Length)
            {
                if (k[p] >= ' ') r += k[p]; else r += '.';
                p = p + 1;
            }
            return r;
        }

        public static string AlphaNum(string k, string w = "_")
        {
            string r = "";
            int p = 0;
            while (p < k.Length)
            {
                if ((k[p] >= 'a' && k[p] <= 'z') || (k[p] >= 'A' && k[p] <= 'Z') || (k[p] >= '0' && k[p] <= '9')) r += k[p]; else r += w;
                p = p + 1;
            }
            return r;
        }

        public static string SplitGet(this string s, char separator, int pos)
        {
            var p = s.Split(separator);
            if (pos > 0) return (pos > p.Length) ? "" : p[pos];
            pos = -pos;
            return (pos > p.Length) ? "" : p[p.Length - pos];
        }


        public static void Compress(string source, string target)
        {
            using (FileStream src = new FileStream(source, FileMode.Open))
            using (FileStream dest = new FileStream(target, FileMode.Create))
            using (GZipStream zipStream = new GZipStream(dest, CompressionMode.Compress, false))
                src.CopyTo(zipStream);
        }
        public static void UnCompress(string source, string target)
        {
            using (FileStream src = new FileStream(source, FileMode.Open))
            using (GZipStream zipStream = new GZipStream(src, CompressionMode.Decompress))
            using (FileStream targetStrean = new FileStream(target, FileMode.Create))
                zipStream.CopyTo(targetStrean);
        }

        static long nco = 0;

        public static string Unique()
        {
            var r = Guid.NewGuid().ToString().Replace("-", "").Replace(":", "").Substring(0, 8) + "_" + nco.ToString();
            nco++;
            var s = "tf_" + (TimeStamp() + "_" + r).ToLower();
            var k = "";
            foreach (var c in s) if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || (c == '_')) k = k + c;
            return k;
        }

        public static string TimeStamp()
        {
            return AlphaNum(DateTime.Now.Format(), "_");
        }


        public static byte[] ReadExactly(this Stream stream, long maxCount)
        {
            using (MemoryStream result = new MemoryStream())
            {
                byte[] buffer = new byte[1024 * 1024];
                int bytesRead = 0;
                long leftToRead = maxCount;

                while ((bytesRead = stream.Read(buffer, 0, leftToRead > int.MaxValue ? int.MaxValue : Convert.ToInt32(leftToRead))) != 0)
                {
                    leftToRead -= bytesRead;
                    result.Write(buffer, 0, bytesRead);
                }

                return result.ToArray();
            }
        }



        public static string ToBase64(this byte[] data)
        {
            return Convert.ToBase64String(data);
        }
        public static string ToISO8601(this DateTime timestamp)
        {
            return timestamp.ToUniversalTime().ToString("yyyyMMdd'T'HH:mm:ss+0000");
        }
        public static byte[] ToByteArray(this string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }
        public static string ChopRightUntil(this string value, char k)
        {
            if (value==null) return "";
            if (value.Length < 1) return "";
            var i = value.Length - 1;
            while (i > 0 && value[i] != k) i--;
            return value.Substring(0, i);
        }


        public static T HttpGet<T>(this string url)
        {
            try
            {
                return HttpGetWrapped<T>(url);
            }
            catch(Exception e)
            {
                throw new Exception("Fetching " + url + ". " + e.Message);
            }
        }

        public static T FileGet<T>(this string url)
        {
            object result = null;
            if (typeof(T) == typeof(string)) result = File.ReadAllText(url);
            if (typeof(T) == typeof(Stream)) result = new FileStream(url, FileMode.Open, FileAccess.Read); ;
            if (typeof(T) == typeof(byte[])) result = File.ReadAllBytes(url);
            if (result != null) return (T)result;
            throw new NotImplementedException("FileGet failed for type " + typeof(T).Name);
        }


        public static T HttpGetWrapped<T>(this string url)
        {
            object result = null;
            if (typeof(T) == typeof(string)) result=(object)(new HttpClient()).GetStringAsync(url).Result;
            if (typeof(T) == typeof(Stream)) result = (new HttpClient()).GetStreamAsync(url).Result;
            if (typeof(T) == typeof(byte[]))
            {
                using (var ms = new MemoryStream())
                {
                    using (var source = (new HttpClient()).GetStreamAsync(url).Result) source.CopyTo(ms);
                    ms.Position = 0;
                    result=ms.ToArray();
                }
            }

            if (result != null) return (T)result;
            throw new NotImplementedException("HttpGet failed for type "+typeof(T).Name);
        }

        public static string UrlPath(string value)
        {
            var m = new Uri(value);
            var p0 = m.GetLeftPart(UriPartial.Path);
            return p0.Substring(0, p0.LastIndexOf('/')) + "/";
        }

        public enum IfNotFound {  returnEmpty,Throw};

        public static string Get(this Microsoft.AspNetCore.Http.IQueryCollection q,string w,IfNotFound def=IfNotFound.Throw)
        {
            string p = q[w];
            if (p != null && p.Length > 0) return p.Trim();
            return def == IfNotFound.returnEmpty ? "" : throw new Exception("Missing query parameter: " + w);
        }

        public static string Get(this Microsoft.AspNetCore.Http.IFormCollection q, string w, IfNotFound def = IfNotFound.Throw)
        {
            string p = q[w];
            if (p != null && p.Length > 0) return p.Trim();
            return def == IfNotFound.returnEmpty ? "" : throw new Exception("Missing form parameter: " + w);
        }

        public static T TryCatch<T>(Func<T> f1, Func<Exception, T> onFail = null)
        {
            
            try
            {
                return f1();
            }
            catch(Exception e)
            {
                return onFail(e);
            }
        }
        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        public static T TryFail<T>(Func<T> f1, string message)
        {

            try
            {
                return f1();
            }
            catch (Exception)
            {
                throw new Exception(message);
            }
        }

        public static string Left(this string value, int length)
        {
            if (value == null) return "";
            if (length > value.Length) return value;
            return value.Substring(0, length);
        }

        public static Dictionary<string, byte[]> Unzip(Stream dataStream)
        {
            var result = new Dictionary<string, byte[]>();
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
                            result.Add(entry.Name.Trim(), memoryStream.ToArray());
                        }
                    }
                }
                return result;
            }
        }


        public static byte[] Zip(Dictionary<string, byte[]> input)
        {
            using (var ms = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(ms, ZipArchiveMode.Create,true))                                 // Leave the MS stream open
                {
                    foreach (var x in input)
                    {
                        var entry = zipArchive.CreateEntry(x.Key,CompressionLevel.Optimal);
                        using (var zipStream = entry.Open())
                        {
                            zipStream.Write(x.Value, 0, x.Value.Length);
                            //zipStream.Close();
                        }
                    }
                }
                ms.Position = 0;
                return ms.ToArray();
            }
        }

        public static void PrintColor(string text,ConsoleColor c=ConsoleColor.Green)
        {
            var cb = Console.ForegroundColor;
            Console.ForegroundColor = c;
            Console.WriteLine(text);
            Console.ForegroundColor = cb;
        }

        
    }
}
