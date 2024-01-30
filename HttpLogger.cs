using Microsoft.Extensions.Logging;

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
