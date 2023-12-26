using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Linq;

namespace JFunc.Utils
{
    
    public static class Crypt
    {

        static string UrlFriendly(string s)
        {
            s= s.Replace('+', '.');
            s= s.Replace('/', '_');
            s= s.Replace('=', '-');
            return s;
        }
        static string UndoUrlFriendly(string s)
        {
            s = s.Replace('.', '+');
            s = s.Replace('_', '/');
            s = s.Replace('-', '=');
            return s;
        }


        public static string Encrypt(string EncryptionKey, string clearText)
        {
            if (clearText.Length < 1) throw new Exception("Some text is needed to crypt");
            byte[] clearBytes = Encoding.UTF8.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    var cryptedText = UrlFriendly(Convert.ToBase64String(ms.ToArray()));
                    if (Decrypt(EncryptionKey, cryptedText) != clearText) throw new Exception("Internal error in AES crypt");
                    return cryptedText;
                }
            }
        }
        public static string Decrypt(string EncryptionKey,string cipherText)
        {
            cipherText = UndoUrlFriendly(cipherText);
            if (cipherText.Length < 1) throw new Exception("Some text is needed to decrypt");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }


        public static byte[] Encrypt(string EncryptionKey, Stream clearText)
        {
            //byte[] clearBytes = Encoding.UTF8.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write)) clearText.CopyTo(cs);
                    return ms.ToArray();
                }
            }
        }

        public static byte[] Decrypt(string EncryptionKey, Stream cipherText)
        {
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write)) cipherText.CopyTo(cs);
                    return ms.ToArray();
                }
            }
        }

    }
}