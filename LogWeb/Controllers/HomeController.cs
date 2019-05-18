using Log;
using LogWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LogWeb.Controllers
{
    public class HomeController : Controller
    {
        Logger _logger;

        public HomeController()
        {
            _logger = new Logger("127.0.0.1", 4396);
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> LogPage(LogSearch req)
        {
            try
            {
                var request = new LogPageReq
                {
                    Start = req.start,
                    Limit = req.limit,
                    Appid = req.appid ?? "",
                    Branch = req.branch ?? "",
                    Category = req.category ?? "",
                    Logkey = req.logkey ?? "",
                    Loglevel = req.loglevel,
                    Model = req.model ?? "",
                    Startime = GetTimeStamp(req.starttime)
                };
                if (req.endtime != null)
                {
                    request.Endtime = GetTimeStamp(req.endtime.Value);
                }
                var res = await _logger.LogPage(request);
                if (res.Code == -1)
                {
                    return Json(new { msg = res.Msg, data = res.Logmodels, code = 0, count = res.Total });
                }
                return Json(new { msg = res.Total == 0 ? "No data" : "Success", data = res.Logmodels, code = 0, count = res.Total });
            }
            catch (Exception ex)
            {
                return Json(new { msg = ex.Message, code = -1 });
            }
        }
        public long GetTimeStamp(DateTime dt)
        {
            TimeSpan ts = dt - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }
    }
}
