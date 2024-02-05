using System;
using System.IO;

namespace jFunc.Azure
{
    public class Storage
    {
        static System.Globalization.NumberFormatInfo separator = new System.Globalization.NumberFormatInfo()
        {
            NumberDecimalDigits = 0,
            NumberGroupSeparator = ","
        };
        
        Messages msg = null;
        string rootFolder = "";
        public int notifyInterval { get; set; } = 30;

        public string Error {  get => msg.error; }
        public Storage(string wUrl,string wSas,string baseFolder)                                               // Define storage
        {
            msg = new Messages(wUrl, wSas);                                                                     // This is a wrapper for HTTP messages
            rootFolder= baseFolder;                                                                             // Root folder
        }

        public bool Exists(string src)
        {
            src = rootFolder + src;
            var blob = msg.Get(src);                                                    // Get file
            return blob != null;
        }


        string fileSize(string dest)
        {
            var f = new FileInfo(dest);
            return (f.Length > 1024000) ? (f.Length / 1024000).ToString("N0") + " Mbytes " : f.Length.ToString("N0") + " bytes";
        }

        public bool Download(string src,string dest)
        {
            msg.error = "";
            src = rootFolder+src;
            var blob = msg.Get(src);                                                    // Get file
            if (blob == null)
            {
                msg.error="File not in Azure: " + src;
                return false;
            }
            return msg.Download(src, dest);
        }

        public string Fetch(string src)
        {
            msg.error = "";
            src = rootFolder + src;
            var blob = msg.Get(src);                                                    // Get file
            if (blob == null) 
            {
                msg.error = "File not in Azure: " + src;
                return "";
            }
            return msg.Fetch(src);
        }


        public bool Delete(string w)
        {
            try
            {
                msg.error = "";
                if (w.EndsWith("/")) throw new Exception("Cannot delete a folder " + w);
                w = rootFolder + w;
                var b = msg.Get(w);
                if (b == null || b.Size > 0) throw new Exception("Cannot delete  " + w);
                msg.Delete(w);
                return true;
            }
            catch(Exception e)
            {
                msg.error = e.Message;
                return true;
            }
        }

        public bool PutBlock(string target, string data, string mime)
        {
            target = rootFolder + target;
            try
            {
                msg.error = "";
                var blob = msg.Get(target);                                                                                                         // Delete previous version of the file
                if (blob != null) msg.Delete(target);
                if (msg.error != "") throw new Exception(msg.error);
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
                msg.CreateBlockAndContent(target,mime, bytes);                                                                                                          // Upload
                if (msg.error != "") throw new Exception(msg.error);                                                                                // If error....
                return true;
            }
            catch (Exception e)
            {
                msg.error = e.Message;
                return false;
            }
        }

        public bool Write(string target,string data)
        {
            target = rootFolder + target; 
            try
            {
                msg.error = "";
                msg.Append(target, data);                                                                                                          // Upload
                if (msg.error != "") throw new Exception(msg.error);                                                                                // If error....
                return true;
            }
            catch (Exception e)
            {
                msg.error = e.Message;
                return false;
            }
        }

        public bool Put(string target,string data)
        {
            target = rootFolder + target;
            try
            {
                msg.error = "";
                var blob = msg.Get(target);                                                                                                         // Delete previous version of the file
                if (blob != null) msg.Delete(target);
                msg.CreateBlob(target);                                                                                                             // Create new BLOB
                if (msg.error != "") throw new Exception(msg.error);
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
                msg.Append(target, bytes);                                                                                                          // Upload
                if (msg.error != "") throw new Exception(msg.error);                                                                                // If error....
                return true;
            }
            catch (Exception e)
            {
                msg.error = e.Message;
                return false;
            }
        }

        public bool Upload(string fileName, string target)
        {
            target=rootFolder+target;
            try
            {
                msg.error = "";
                var blob = msg.Get(target);                                                                                                         // Delete previous version of the file
                if (blob!=null) msg.Delete(target);                                                                             

                msg.CreateBlob(target);                                                                                                             // Create new BLOB
                if (msg.error != "") throw new Exception(msg.error);
                
                const int BUFFSIZE = 30000 * 1024;                                                                                                  // Upload CHUNK size = 30Mbytes
                using (BinaryReader binReader = new BinaryReader(File.Open(fileName, FileMode.Open,FileAccess.Read,FileShare.Read)))                // Read file        
                {
                    var buffer = binReader.ReadBytes(BUFFSIZE);                                                                                     // Read a CHUNK                                                              
                    while (buffer.Length > 0)                                                                                                       // While something is read
                    {
                        msg.Append(target, buffer);                                                                                                 // Upload
                        if (msg.error != "") throw new Exception(msg.error);                                                                        // If error....
                        buffer = binReader.ReadBytes(BUFFSIZE);                                                                                     // Read next CHUNK
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                msg.error = e.Message;
                return false;
            }
        }

        public Blob[] List(string folder = "")
        {
            msg.error= "";
            if (folder == null) folder = "";
            if (folder.EndsWith("/")) folder= folder.Substring(0, folder.Length - 1);   
            return msg.List(rootFolder + folder);
        }
    }
}
