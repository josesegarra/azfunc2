using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jFunc
{
    internal class HttpLogger
    {
        internal static ILogger logger = null;
        internal static void Log(string s)
        {
            if (logger == null) return;
            logger.Log(LogLevel.Critical, s);
        }

    }
}
