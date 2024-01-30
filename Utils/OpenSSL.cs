using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace jFunc
{
    // https://stackoverflow.com/questions/69470583/it-is-possible-to-decrypt-aes-password-protected-file-in-c-sharp-dotnet-6-encr/69471070#69471070
    // https://stackoverflow.com/questions/76508136/c-sharp-encrypt-decrypt-aes-256-cbc-with-pbkdf2-from-openssl
    public class OpenSSL
    {
        public string Encrypt(string plainText, string passphrase)
        {
            byte[] salt = new byte[8];                                                                                                      // Let's generate a random SALT                    
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);

            var iterations = 10000;                                                                                                         // From the PASS+SALT+ITERATIONS let's derive a KEY of 48 bytes using Pbkdf2       
            var rfcKey = Rfc2898DeriveBytes.Pbkdf2(passphrase, salt, iterations, HashAlgorithmName.SHA256, 32 + 16);

            var aesBytes = EncryptWithAes(plainText, rfcKey.Take(32).ToArray(), rfcKey.Skip(32).Take(16).ToArray());                        // Let's use the first 32 bytes as KEY and the last 16 as Initialization Vector, and encrypt using AES

            using (var buffer= new MemoryStream())                                                                                          // Let's encode using OPENSSL magic number
            {
                buffer.Write(Encoding.ASCII.GetBytes("Salted__"));
                buffer.Write(salt);
                buffer.Write(aesBytes);
                buffer.Position = 0;
                return Convert.ToBase64String(buffer.ToArray());
            }
        }

        byte[] EncryptWithAes(string text, byte[] key, byte[] iv)
        {
            Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.BlockSize = 128;
            aes.Key = key;
            aes.IV = iv;

            using (var encryptionStream = new MemoryStream())
            {
                using (CryptoStream encrypt = new CryptoStream(encryptionStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] utfD1 = new System.Text.UTF8Encoding(false).GetBytes(text);
                    encrypt.Write(utfD1, 0, utfD1.Length);
                    encrypt.FlushFinalBlock();
                    encrypt.Close();
                    return encryptionStream.ToArray();
                }
            }
        }
    }
}

