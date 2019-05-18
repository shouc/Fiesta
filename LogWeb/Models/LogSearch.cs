using System;

namespace LogWeb.Models
{
    public class LogSearch
    {
        
        public int page { get; set; }

        public int limit { get; set; }

        public int start
        {
            get
            {
                return (page - 1) * limit;
            }
        }

        public int end
        {
            get
            {
                return start + limit;
            }
        }

        public DateTime starttime { get; set; } = DateTime.Now.Date;

        public DateTime? endtime { get; set; }

        public string appid { get; set; }

        public string branch { get; set; }

        public string model { get; set; }

        public string category { get; set; }

        public string logkey { get; set; }

        public int loglevel { get; set; }

    }
}