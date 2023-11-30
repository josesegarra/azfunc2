using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using jFunc;

namespace jFunc.Akamai
{
    public enum ChecksumAlgorithm { SHA256, SHA1, MD5 };
    public enum KeyedHashAlgorithm { HMACSHA256, HMACSHA1, HMACMD5 };

    public class HashType
    {
        public static HashType SHA256 = new HashType(ChecksumAlgorithm.SHA256);
        public ChecksumAlgorithm Checksum { get; private set; }
        private HashType(ChecksumAlgorithm checksum)
        {
            this.Checksum = checksum;
        }
    }

    public class SignType
    {
        public static SignType HMACSHA256 = new SignType("EG1-HMAC-SHA256", KeyedHashAlgorithm.HMACSHA256);

        public string Name { get; private set; }
        public KeyedHashAlgorithm Algorithm { get; private set; }
        private SignType(string name, KeyedHashAlgorithm algorithm)
        {
            this.Name = name;
            this.Algorithm = algorithm;
        }
    }
    internal static class AkamaiUtils
    {
        static SignType SignVersion = SignType.HMACSHA256;
        static HashType HashVersion = HashType.SHA256;


        /// Computes the hash of a given InputStream. This is a wrapper over the HashAlgorithm crypto functions.
        public static byte[] ComputeHash(Stream stream, ChecksumAlgorithm hashType = ChecksumAlgorithm.SHA256, long? maxBodySize = null)
        {
            if (stream == null) return null;

            using (var algorithm = HashAlgorithm.Create(hashType.ToString()))
                if (maxBodySize != null && maxBodySize > 0) return algorithm.ComputeHash(stream.ReadExactly((long)maxBodySize)); else return algorithm.ComputeHash(stream);

        }

        public static byte[] ComputeHash(string value, Encoding encoding, ChecksumAlgorithm hashType = ChecksumAlgorithm.SHA256)
        {
            using (var algorithm = HashAlgorithm.Create(hashType.ToString()))
            {
                byte[] bytes = encoding.GetBytes(value);
                return algorithm.ComputeHash(bytes);
            }
        }

        internal static string GetRequestData(string method, Uri uri, string data)
        {
            String headers = "";                                                            // We are not encoding headers in the request
            String bodyHash = method == "POST" ? GetRequestDataHash(data) : "";
            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t",method.ToUpper(),uri.Scheme,uri.Host,uri.PathAndQuery,headers,bodyHash);
        }

        internal static string GetRequestDataHash(string data)
        {
            return ComputeHash(data,Encoding.UTF8,HashType.SHA256.Checksum).ToBase64();
            /*
            Stream requestStream
            if (requestStream == null) return string.Empty;
            string streamHash = requestStream.ComputeHash(JsRunTime.HashType.SHA256.Checksum, maxBodyHashSize).ToBase64();
            requestStream.Seek(0, SeekOrigin.Begin);
            return streamHash;
            */
        }

        internal static string GetAuthDataValue(ClientCredential credential, string timestamp)
        {
            string nonce = Guid.NewGuid().ToString().ToLower();
            return string.Format("{0} client_token={1};access_token={2};timestamp={3};nonce={4};",SignVersion.Name,credential.ClientToken,credential.AccessToken,timestamp,nonce);
        }

        /// Computes the HMAC hash of a given byte[]. This is a wrapper over the Mac crypto functions.
        public static byte[] ComputeKeyedHash(this byte[] data, string key, KeyedHashAlgorithm hashType = KeyedHashAlgorithm.HMACSHA256)
        {
            if (data == null) return null;
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            using (var algorithm = HMAC.Create(hashType.ToString()))
            {
                algorithm.Key = key.ToByteArray();
                return algorithm.ComputeHash(data);
            }
        }

        internal static string GetAuthorizationHeaderValue(ClientCredential credential, string timestamp, string authData, string requestData)
        {
            string signingKey = timestamp.ToByteArray().ComputeKeyedHash(credential.Secret, SignVersion.Algorithm).ToBase64();
            string authSignature = string.Format("{0}{1}", requestData, authData).ToByteArray().ComputeKeyedHash(signingKey, SignVersion.Algorithm).ToBase64();
            return string.Format("{0}signature={1}", authData, authSignature);
        }
    }
}
