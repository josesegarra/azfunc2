using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace jFunc.Akamai
{
    public class AkamaiResponse
    {
        public Dictionary<string,object> Headers { get; } = new Dictionary<string,object>();
        public int Status = 0;
        public string Content { get;  }

        internal AkamaiResponse(HttpResponseMessage message)
        {
            Status=(int)message.StatusCode;
            foreach (var a in message.Headers)
            {
                if (a.Value.GetType() is IEnumerable)
                {
                    var f = ((IEnumerable)a.Value).Cast<string>();
                    if (f.Count() == 1) Headers[a.Key] = f.First(); else Headers[a.Key] = f;
                }
                else Headers[a.Key] = a.Value;
            }
            Content =message.Content.ReadAsStringAsync().Result.Trim();
        }
    }
}
