using Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;

namespace LogServer
{
    public class LogDbHelper
    {

        public static void BatchInsert(List<LogModel> logModels)
        {
            DbProviderFactory Dbfactory = SQLiteFactory.Instance;
            using (DbConnection dbConn = Dbfactory.CreateConnection())
            {
                var dbname = DateTime.Now.ToString("yyyyMM") + ".db";
                dbConn.ConnectionString = $"data source ={dbname};Synchronous=Off;Journal Mode=WAL;";
                dbConn.Open();
                string sql = @"create table if not exists LogModel([id] INTEGER PRIMARY KEY autoincrement,  [appid] nvarchar, [branch] nvarchar,[model] nvarchar,
                            [category] nvarchar,[logkey] nvarchar,[loglevel] int,[logmsg] text,[logtime] bigint ,[sendmail] nvarchar);
                            CREATE INDEX if not exists time_index ON LogModel (logtime);
                            CREATE INDEX if not exists appid_index ON LogModel (appid);
                            CREATE INDEX if not exists branch_index ON LogModel (branch);
                            CREATE INDEX if not exists loglevel_index ON LogModel (loglevel);
                            CREATE INDEX if not exists logkey_index ON LogModel (logkey);";
                using (DbCommand cmd = dbConn.CreateCommand())
                {
                    cmd.Connection = dbConn;
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();

                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    using (DbTransaction trans = dbConn.BeginTransaction())
                    {
                        cmd.CommandText = @"insert into LogModel ([appid],[branch],[model],[category],[logkey],[loglevel],[logmsg],[logtime],[sendmail])
                                       values (@appid,@branch,@model,@category,@logkey,@loglevel,@logmsg,@logtime,@sendmail)";
                        SQLiteParameter[] parameter = new SQLiteParameter[9];
                        foreach (var item in logModels)
                        {
                            parameter[0] = new SQLiteParameter("@appid", item.Appid);
                            parameter[1] = new SQLiteParameter("@branch", item.Branch);
                            parameter[2] = new SQLiteParameter("@model", item.Model);
                            parameter[3] = new SQLiteParameter("@category", item.Category);
                            parameter[4] = new SQLiteParameter("@logkey", item.Logkey);
                            parameter[5] = new SQLiteParameter("@loglevel", item.Loglevel);
                            parameter[6] = new SQLiteParameter("@logmsg", item.Logmsg);
                            parameter[7] = new SQLiteParameter("@logtime", item.Logtime);
                            parameter[8] = new SQLiteParameter("@sendmail", item.Sendmail);
                            cmd.Parameters.AddRange(parameter);
                            cmd.ExecuteNonQuery();
                        }
                        trans.Commit();
                        dbConn.Close();
                        watch.Stop();
                        Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}Inserted {logModels.Count} reecords in " + watch.ElapsedMilliseconds);
                    }
                }
            }
        }

        public static DataTable ExecuteDataTable(string dbname, string sql, List<SQLiteParameter> parameter)
        {
            var result = new DataTable();
            DbProviderFactory Dbfactory = SQLiteFactory.Instance;
            using (DbConnection dbConn = Dbfactory.CreateConnection())
            {
                dbConn.ConnectionString = $"data source ={dbname};Synchronous=Off;Journal Mode=WAL;";
                dbConn.Open();
                using (DbCommand cmd = dbConn.CreateCommand())
                {
                    cmd.Connection = dbConn;
                    cmd.CommandText = sql;
                    if (parameter != null && parameter.Count > 0)
                    {
                        cmd.Parameters.AddRange(parameter.ToArray());
                    }
                    using (SQLiteDataAdapter da = new SQLiteDataAdapter((SQLiteCommand)cmd))
                    {
                        da.Fill(result);
                        dbConn.Close();
                        return result;
                    }
                }
            }
        }

        public static object ExecuteScalar(string dbname, string sql, List<SQLiteParameter> parameter)
        {
            object result = null;
            DbProviderFactory Dbfactory = SQLiteFactory.Instance;
            using (DbConnection dbConn = Dbfactory.CreateConnection())
            {
                dbConn.ConnectionString = $"data source ={dbname};Synchronous=Off;Journal Mode=WAL;Cache Size=5000;";
                dbConn.Open();
                using (DbCommand cmd = dbConn.CreateCommand())
                {
                    cmd.Connection = dbConn;
                    cmd.CommandText = sql;
                    if (parameter != null && parameter.Count > 0)
                    {
                        cmd.Parameters.AddRange(parameter.ToArray());
                    }
                    result = cmd.ExecuteScalar();
                    dbConn.Close();
                    return result;
                }
            }
        }

        public static List<LogModel> GetLogModels(long start, int limit, long starttime, long endtime, int loglevel, out long total,
          string appid = "", string branch = "", string model = "", string category = "", string logkey = "")
        {
            var startdate = GetTime(starttime);
            var db = startdate.ToString("yyyyMM") + ".db";
            if (File.Exists(db) == false)
            {
                throw new Exception("No log");
            }
            var sql = $"select * from LogModel where logtime>=@starttime";
            var param = new List<SQLiteParameter>();
            param.Add(new SQLiteParameter("@starttime", starttime));
            if (endtime != 0)
            {
                var enddate = GetTime(endtime);
                if (startdate.ToString("yyyyMM") != enddate.ToString("yyyyMM"))
                {
                    throw new Exception("Query months must be same");
                }
                sql += " and logtime<=@endtime";
                param.Add(new SQLiteParameter("@endtime", endtime));
            }
            if (string.IsNullOrWhiteSpace(appid) == false)
            {
                sql += " and appid=@appid";
                param.Add(new SQLiteParameter("@appid", appid));
            }
            if (string.IsNullOrWhiteSpace(branch) == false)
            {
                sql += $" and branch=@branch";
                param.Add(new SQLiteParameter("@branch", branch));
            }
            if (loglevel > 0)
            {
                sql += $" and loglevel=@loglevel";
                param.Add(new SQLiteParameter("@loglevel", loglevel));
            }
            if (string.IsNullOrWhiteSpace(model) == false)
            {
                sql += $" and model=@model";
                param.Add(new SQLiteParameter("@model", model));
            }
            if (string.IsNullOrWhiteSpace(category) == false)
            {
                sql += $" and category=@category";
                param.Add(new SQLiteParameter("@category", category));
            }
            if (string.IsNullOrWhiteSpace(logkey) == false)
            {
                sql += $" and logkey=@logkey";
                param.Add(new SQLiteParameter("@logkey", logkey));
            }
            var totalsql = $"select count(1) from ({sql})";
            total = long.Parse(ExecuteScalar(db, totalsql, param).ToString());
            sql += $" order by id desc limit {limit} offset {start}";
            var table = ExecuteDataTable(db, sql, param);
            return TableToLogModel(table);
        }
        private static List<LogModel> TableToLogModel(DataTable data)
        {
            var result = new List<LogModel>();
            for (int i = 0; i < data.Rows.Count; i++)
            {
                var row = data.Rows[i];
                var model = new LogModel();
                model.Id = row["id"] == DBNull.Value ? 0 : long.Parse(row["id"].ToString());
                model.Appid = row["appid"] == DBNull.Value ? "" : row["appid"].ToString();
                model.Branch = row["branch"] == DBNull.Value ? "" : row["branch"].ToString();
                model.Category = row["category"] == DBNull.Value ? "" : row["category"].ToString();
                model.Model = row["model"] == DBNull.Value ? "" : row["model"].ToString();
                model.Logkey = row["logkey"] == DBNull.Value ? "" : row["logkey"].ToString();
                model.Loglevel = row["loglevel"] == DBNull.Value ? -1 : int.Parse(row["loglevel"].ToString());
                model.Logmsg = row["logmsg"] == DBNull.Value ? "" : row["logmsg"].ToString();
                model.Logtime = row["logtime"] == DBNull.Value ? 0 : long.Parse(row["logtime"].ToString());
                result.Add(model);
            }
            return result;
        }
        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }

        public static DateTime GetTime(long timeStamp)
        {
            DateTime startTime = new DateTime(1970, 1, 1);
            return startTime.AddMilliseconds(timeStamp);
        }
    }
}
