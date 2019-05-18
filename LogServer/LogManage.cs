using Grpc.Core;
using Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LogServer
{
    public class LogManage
    {
        Server server = null;
        bool run = true;
        public void Start()
        {
            server = new Server
            {
                Services = { Greeter.BindService(new LogImpl()) },
                Ports = { new ServerPort(LogConfig.host, LogConfig.port, ServerCredentials.Insecure) }
            };
            server.Start();
            run = true;
            LogPersistence();
            SendMail();
        }

        public void Stop()
        {
            LogData.AllData.Clear();
            LogData.ErrorData.Clear();
            run = false;
            server.ShutdownAsync().Wait();
        }

        private void LogPersistence()
        {
            var thread = new Thread(
            t =>
            {
                var list = new List<LogModel>();
                while (run)
                {
                    try
                    {
                        LogModel logmodel = null;
                        var isok = LogData.AllData.TryDequeue(out logmodel);
                        if (isok)
                        {
                            list.Add(logmodel);
                            if (list.Count >= 5000)
                            {
                                LogDbHelper.BatchInsert(list);
                                list.Clear();
                                Thread.Sleep(1000);
                            }
                        }
                        else
                        {
                            if (list.Any())
                            {
                                LogDbHelper.BatchInsert(list);
                                list.Clear();
                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            });
            thread.Start();
        }

        private void SendMail()
        {
            var thread = new Thread(
           t =>
           {
               while (run)
               {
                   try
                   {
                       LogModel logmodel = null;
                       var isok = LogData.ErrorData.TryDequeue(out logmodel);
                       if (isok)
                       {
                           LogMailHelper.SendErrorMail(logmodel);
                       }
                       Thread.Sleep(1000);
                   }
                   catch (Exception ex)
                   {
                   }
               }
           });
            thread.Start();
        }
    }
}
