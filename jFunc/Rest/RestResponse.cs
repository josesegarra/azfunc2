using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace jFunc.Rest
{
    public class RestResponse
    {
        public Dictionary<string, object> Headers { get; } = new Dictionary<string, object>();
        public int Status = 0;
        public string Content { get; }

        internal RestResponse(HttpResponseMessage message)
        {
            Status = (int)message.StatusCode;
            foreach (var a in message.Headers)
            {
                if (a.Value.GetType() is IEnumerable)
                {
                    var f = a.Value.Cast<string>();
                    if (f.Count() == 1) Headers[a.Key] = f.First(); else Headers[a.Key] = f;
                }
                else Headers[a.Key] = a.Value;
            }
            Content = message.Content.ReadAsStringAsync().Result.Trim();
        }
    }
}
