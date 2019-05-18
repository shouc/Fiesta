using Grpc.Core;
using Log;
using System;
using System.Threading.Tasks;

namespace LogServer
{
    public class LogImpl : Greeter.GreeterBase
    {
        
        public override Task<LogRes> Log(LogModel request, ServerCallContext context)
        {
            request.Logtime = LogDbHelper.GetTimeStamp();
            if (request.Loglevel == 4)
            {
                LogData.ErrorData.Enqueue(request);
                LogData.AllData.Enqueue(request);
            }
            else
            {
                LogData.AllData.Enqueue(request);
            }
            return Task.FromResult(new LogRes { Code = 200, Msg = "success" });
        }

        public override Task<LogPageRes> SearchLog(LogPageReq request, ServerCallContext context)
        {
            long total = 0;
            var result = new LogPageRes();
            try
            {
                var list = LogDbHelper.GetLogModels(request.Start, request.Limit, request.Startime, request.Endtime, request.Loglevel, out total
               , request.Appid, request.Branch, request.Model, request.Category, request.Logkey);
                result.Logmodels.AddRange(list);
                result.Total = total;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Msg = ex.Message;
            }
            return Task.FromResult(result);
        }
    }
}
