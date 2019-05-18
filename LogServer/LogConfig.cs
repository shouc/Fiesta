using System.Collections.Generic;

namespace LogServer
{
    public class LogConfig
    {
        public const string host = "0.0.0.0";
        public const int port = 4396;
        public const string bindLogggerName = "Logger";
        public const string bindLogReadName = "LogRead";

        public static string mailSubject = "Help!! System is down!";
        public static string mailSmtpServer = "CHANGE_IT";
        public static int mailport = 465;
        public static string mailfrom = "CHANGE_IT";
        public static string mailpassword = "CHANGE_IT";
        public static bool mailenableSSL = true;

        public static Dictionary<string, string> Apps = new Dictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("CHANGE_IT (ID)","CHANGE_IT (NAME)"),
        });
    }
}
