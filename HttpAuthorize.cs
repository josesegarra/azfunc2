using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using jFunc.Js;
using System.Dynamic;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace jFunc
{

    public static class HttpAuthorize
    {
        internal static string DecryptValue(string value)
        {
            value = (value ?? "").Trim();
            if (value.Length < 3) throw new Exception("Bad [_value] in query string");
            var key = (Environment.GetEnvironmentVariable("key") ?? "").Trim();
            if (key.Length< 3) throw new Exception("Bad key ");
            return Crypt.Decrypt(key, value);
        }

    }
}
