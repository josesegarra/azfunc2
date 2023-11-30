using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jFunc.Akamai
{
    public class ClientCredential
    {
        public string ClientToken { get; private set; }
        public string AccessToken { get; private set; }

        public string Secret { get; private set; }

        public ClientCredential(string clientToken, string accessToken, string secret)
        {
            if (string.IsNullOrEmpty(clientToken) || string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(secret)) throw new ArgumentNullException("Missing credential info");
            ClientToken = clientToken;
            AccessToken = accessToken;
            Secret = secret;
        }
    }
}