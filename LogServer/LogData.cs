using Log;
using System.Collections.Concurrent;

namespace LogServer
{
    public class LogData
    {
        
        public static ConcurrentQueue<LogModel> AllData = new ConcurrentQueue<LogModel>();

        public static ConcurrentQueue<LogModel> ErrorData = new ConcurrentQueue<LogModel>();
    }
}
