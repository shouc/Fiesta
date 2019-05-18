using Log;
using MailKit.Net.Smtp;
using MimeKit;

namespace LogServer
{
    public class LogMailHelper
    {
        public static void SendErrorMail(LogModel log)
        {
            var appname = "Unknown Project";
            LogConfig.Apps.TryGetValue(log.Appid, out appname);
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Log system", LogConfig.mailfrom));
            message.To.Add(new MailboxAddress(appname, log.Sendmail));
            message.Subject = appname + "|" + log.Branch + LogConfig.mailSubject;

            message.Body = new TextPart("plain")
            {
                Text = log.Logmsg
            };
            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                client.Connect(LogConfig.mailSmtpServer, LogConfig.mailport, true);

                client.Authenticate(LogConfig.mailfrom, LogConfig.mailpassword);

                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
