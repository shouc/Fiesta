using System;
using System.Threading.Tasks;
using Grpc.Core;
using Log;
using Topshelf;

namespace LogServer
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<LogManage>(s =>
                {
                    s.ConstructUsing(name => new LogManage());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.SetDescription("Logging");
                x.SetDisplayName("LogCenter");
                x.SetServiceName("LogCenter");
            });
        }
    }
}
