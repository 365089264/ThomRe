using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Net;
using System.IO;
using System.Data;
using System.Data.SqlClient;

namespace CNE.Scheduler.Extension
{
    public class Time
    {
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
        public Time(int h, int m, int s)
        {
            this.Minute = m;
            this.Second = s;
            this.Hour = h;
        }
        public static  bool operator <(Time h, DateTime dt)
        {
            Time time = dt.GetTime();
            long TotalSecond_dt = dt.Hour * 3600 + dt.Minute * 60 + dt.Second;
            long TotalSecond_this = h.Hour * 3600 + h.Minute * 60 + h.Second;
            if (TotalSecond_this < TotalSecond_dt)
                return true;
            else
                return false;
        }
        public static  bool operator >(Time h, DateTime dt)
        {
            if (h < dt)
            {
                return false;
            }
            return true;
        }
        public static  bool operator ==(Time h, DateTime dt)
        {
            if (h < dt || h > dt)
            {
                return false;
            }
            else
                return true;
        }
        public static  bool operator !=(Time h, DateTime dt)
        {
            if (h < dt || h > dt)
            {
                return true;
            }
            else
                return false;
        }
        public override string ToString()
        {
            string hStr = this.Hour < 10 ? "0" + this.Hour + "" : this.Hour + "";
            string mStr = this.Minute < 10 ? "0" + this.Minute + "" : this.Minute + "";
            string sStr = this.Second < 10 ? "0" + this.Second + "" : this.Second + "";
            return hStr + ":" + mStr + ":" + sStr;
        }
    }
    public class Sail_Traval_Manager : BaseDataHandle
    {
        private static string PageUrl = ConfigurationManager.AppSettings["Sail_Traval_url"];
        private static Time beginTime_mor = new Time(9, 0, 0);
        private static Time endTime_mor = new Time(11, 35, 0);
        private static Time beginTime_after = new Time(13, 30, 0);
        private static Time endTime_after = new Time(15, 5, 0);
        public string FetchDateTime { get; set; }
        public string MasterCode { get; set; }
        public bool IsTradeTime { get; set; }
        public bool IsWeekend
        {
            get
            {
                if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                    return true;
                else
                    return false;

            }
        }
        private bool CanFetch
        {
            get
            {
                if ((beginTime_mor == DateTime.Now || (beginTime_mor < DateTime.Now && endTime_mor > DateTime.Now) || (endTime_mor == DateTime.Now)) || ((beginTime_after < DateTime.Now && endTime_after > DateTime.Now) || (beginTime_after == DateTime.Now) || (endTime_after == DateTime.Now)))
                    return true;
                else
                    return false;
            }
        }
        private Time TranslateTime(string str)
        {
            //110329 
            if (str.Length != 6)
            {
                throw new Exception("Time format error");
            }
            string shour = str.Substring(0, 2).StartsWith("0") ? str.Substring(1, 1) : str.Substring(0, 2);
            string sminute = str.Substring(2, 2).StartsWith("0") ? str.Substring(3, 1) : str.Substring(2, 2);
            string ssecond = str.Substring(4, 2).StartsWith("0") ? str.Substring(5, 1) : str.Substring(4, 2);

            return new Time(Convert.ToInt32(shour), Convert.ToInt32(sminute), Convert.ToInt32(ssecond));
        }
        private string TranslateData(string str)
        {
            //110329 
            if (str.Length != 8)
            {
                throw new Exception("Date format error");
            }
            string year = str.Substring(0, 4);
            string month = str.Substring(4, 2);
            string day = str.Substring(6, 2);

            return year + "-" + month + "-" + day;
        }
        private string TranslateDateTime(string str)
        {
            if (str.Length != 12)
            {
                throw new Exception("DataTime format error");
            }
            string year = str.Substring(0, 4);
            string month = str.Substring(4, 2).StartsWith("0") ? str.Substring(5, 1) : str.Substring(4, 2);
            string day = str.Substring(6, 2).StartsWith("0") ? str.Substring(7, 1) : str.Substring(6, 2);
            string hour = str.Substring(8, 2).StartsWith("0") ? str.Substring(9, 1) : str.Substring(8, 2);
            string minute = str.Substring(10, 2).StartsWith("0") ? str.Substring(11, 1) : str.Substring(10, 2);
            
            return year + "-" + month + "-" + day + " " + hour + ":" + minute+":00" ;
        }
        private string GetUrString(StringBuilder sb)
        {
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["smmport"]);
            string address = ConfigurationManager.AppSettings["smmAddress"].ToString();

            try
            {
                WebRequest request = WebRequest.Create(PageUrl);
                WebProxy proxy = new WebProxy(address, port);
                request.Proxy = proxy;
                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream, System.Text.Encoding.GetEncoding("gb2312"));

                var str = reader.ReadToEnd();
                reader.Close();
                return str;
            }
            catch (Exception e)
            {
                sb.AppendFormat("[" + e.Message + "{0}]", DateTime.Now.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss"));
                return "";
            }
        }
        private DataTable CreateTableSchema()
        {
            DataTable tb = new DataTable();
            tb.Columns.Add("code");
            tb.Columns.Add("codeName");
            tb.Columns.Add("yesterdayClosePrice");
            tb.Columns.Add("yesterdayShettlePrice");
            tb.Columns.Add("openPrice");
            tb.Columns.Add("highestPrice");
            tb.Columns.Add("lowestPrice");
            tb.Columns.Add("newestPrice");
            tb.Columns.Add("averagePrice");
            tb.Columns.Add("JiaoGeDate");
            tb.Columns.Add("zhangdie");
            tb.Columns.Add("TurnoverAmount");
            tb.Columns.Add("ChengjiaoJine");
            tb.Columns.Add("chicang");
            tb.Columns.Add("BuOnePrice");
            tb.Columns.Add("BuyOneAmount");
            tb.Columns.Add("SellOnePrice");
            tb.Columns.Add("selloneAmount");
            tb.Columns.Add("chengjiaotime");
            tb.Columns.Add("createDateTime");
            tb.Columns.Add("masterCode");
            tb.Columns.Add("isTradeTime");
            tb.Columns.Add("FetchTime");
            return tb;
        }
        private DataTable FillDataTable(List<string> list)
        {
            DataTable tb = CreateTableSchema();
            foreach (string str in list)
            {
                DataRow dr = tb.NewRow();
                //CC1410,º£áµ1410,189800,191000,189800,189800,189000,189500,189500,20141017,-0.79,24,28002225,625, 177700,2,204300,2,112302
                string[] dataArr = str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                dr["code"] = dataArr[0];
                dr["codeName"] = dataArr[1];
                dr["yesterdayClosePrice"] = dataArr[2];
                dr["yesterdayShettlePrice"] = dataArr[3];
                dr["openPrice"] = dataArr[4];
                dr["highestPrice"] = dataArr[5];
                dr["lowestPrice"] = dataArr[6];
                dr["newestPrice"] = dataArr[7];
                dr["averagePrice"] = dataArr[8];
                dr["JiaoGeDate"] = TranslateData(dataArr[9]);
                dr["zhangdie"] = dataArr[10] + "%";
                dr["TurnoverAmount"] = dataArr[11];
                dr["ChengjiaoJine"] = dataArr[12];
                dr["chicang"] = dataArr[13];
                dr["BuOnePrice"] = dataArr[14];
                dr["BuyOneAmount"] = dataArr[15];
                dr["SellOnePrice"] = dataArr[16];
                dr["selloneAmount"] = dataArr[17];
                dr["chengjiaotime"] = TranslateTime(dataArr[18]).ToString();
                dr["createDateTime"] = FetchDateTime;
                dr["masterCode"] = MasterCode;
                dr["isTradeTime"] = IsTradeTime;
                dr["FetchTime"] = DateTime.Now;
                tb.Rows.Add(dr);
            }
            return tb;
        }
        private List<string> Pre_Manager(string str)
        {
            List<string> list = new List<string>();
            string[] strs = str.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strs.Length - 3; i++)
            {
                list.Add(strs[i]);
            }
            FetchDateTime = TranslateDateTime(strs[strs.Length - 3]);
            MasterCode = strs[strs.Length - 2];
            IsTradeTime = Convert.ToInt32(strs[strs.Length - 1])==1?true :false;
            return list;
        }
        public void Manager(StringBuilder sb)
        {

           
            
                string pageStr = GetUrString(sb);
                List<string> infor = Pre_Manager(pageStr);
                if (infor.Count == 0)
                {
                    sb.Append("[data not exists]");
                }
                else
                {
                    DataTable tb = FillDataTable(infor);
                    using (var sqlBulk = new SqlBulkCopy(Connectionstr, SqlBulkCopyOptions.UseInternalTransaction) { DestinationTableName = "[dbo].[Sail_Travel_SH]" })
                    {
                        sqlBulk.WriteToServer(tb);
                        sqlBulk.Close();
                    }
                }
            
          

        }
    }
}
