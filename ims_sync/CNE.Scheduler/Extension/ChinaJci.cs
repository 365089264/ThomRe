using System;
using System.Linq;
using System.Data;
using CNE.Scheduler.Extension.Model;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Globalization;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;
using OracleDataAdapter = Oracle.ManagedDataAccess.Client.OracleDataAdapter;
using OracleParameter = Oracle.ManagedDataAccess.Client.OracleParameter;

namespace CNE.Scheduler.Extension
{
    public class ChinaJci : BaseDataHandle
    {
        public void SyncChinaJciData(ref StringBuilder sbSync, string reDate)
        {
            string baseUrl = "http://wap.chinajci.com/app/reuters.aspx";
            string url = baseUrl + "?JCIKEY=GNJG&JCIPASSWORD=HUIYI68751628&RE_DATE=" + reDate;
            string str = GetRequestStr(url).Replace("\r", "").Replace("\n", "").Replace("\t", "");
            str = str.Substring(0, str.IndexOf("<!DOCTYPE html>", System.StringComparison.Ordinal));
            if (!string.IsNullOrEmpty(str))
            {
                if (str == "Error")
                {
                    sbSync.Append(" no synchronized from " + url + " to ChinaJciInternalPrice table.\n" + "\r\n");
                }
                else
                {
                    var list = ((List<ChinaJciInternalPrice>)JsonToObject(str, new List<ChinaJciInternalPrice>())).Distinct().ToList();
                    InsertChinaJciInternalPrice(list);
                    sbSync.Append(list.Count + " rows have been synchronized from " + url + " to ChinaJciInternalPrice table.\n" + "\r\n");
                }


            }
            string url2 = baseUrl + "?JCIKEY=GWJG&JCIPASSWORD=HUIYI68751628&RE_DATE=" + reDate;
             str = GetRequestStr(url2).Replace("\r", "").Replace("\n", "").Replace("\t", "");
             str = str.Substring(0, str.IndexOf("<!DOCTYPE html>", System.StringComparison.Ordinal));
             if (!string.IsNullOrEmpty(str))
             {
                 if (str == "Error")
                 {
                     sbSync.Append(" no synchronized from " + url2 + " to ChinaJciImportPrice table.\n" +
                                   "\r\n");
                 }
                 else
                 {
                     var list = ((List<ChinaJciImportPrice>)JsonToObject(str, new List<ChinaJciImportPrice>())).Distinct().ToList();
                     InsertChinaJciImportPrice(list);
                     sbSync.Append(list.Count + " rows have been synchronized from " + url2 +
                                   " to ChinaJciImportPrice table.\n" + "\r\n");

                 }
             }
             string url3 = baseUrl + "?JCIKEY=DDKC&JCIPASSWORD=HUIYI68751628&RE_DATE=" + reDate;
             str = GetRequestStr(url3).Replace("\r", "").Replace("\n", "").Replace("\t", "");
             str = str.Substring(0, str.IndexOf("<!DOCTYPE html>", System.StringComparison.Ordinal));
             if (!string.IsNullOrEmpty(str))
             {
                 if (str == "Error")
                 {
                     sbSync.Append(" no synchronized from " + url3 + " to ChinaJciSoybeanStocksDaily table.\n" +
                                   "\r\n");
                 }
                 else
                 {
                     var list =
                         ((List<ChinaJciSoybeanStocksDaily>)JsonToObject(str, new List<ChinaJciSoybeanStocksDaily>())).Distinct().ToList();
                     InsertChinaJciSoybeanStocksDaily(list);
                     sbSync.Append(list.Count + " rows have been synchronized from " + url3 +
                                   " to ChinaJciSoybeanStocksDaily table.\n" + "\r\n");

                 }
             }
             string url4 = baseUrl + "?JCIKEY=DDYZLR&JCIPASSWORD=HUIYI68751628&RE_DATE=" + reDate;
             str = GetRequestStr(url4).Replace("\r", "").Replace("\n", "").Replace("\t", "");
             str = str.Substring(0, str.IndexOf("<!DOCTYPE html>", System.StringComparison.Ordinal));
             if (!string.IsNullOrEmpty(str))
             {
                 if (str == "Error")
                 {
                     sbSync.Append(" no synchronized from " + url4 + " to ChinaJciSoybeanCrushMarginsD table.\n" +
                                   "\r\n");
                 }
                 else
                 {
                     var list =
                         ((List<ChinaJciSoybeanCrushMarginsDaily>)
                             JsonToObject(str, new List<ChinaJciSoybeanCrushMarginsDaily>())).Distinct().ToList();
                     InsertChinaJciSoybeanCrushMarginsDaily(list);
                     sbSync.Append(list.Count + " rows have been synchronized from " + url4 +
                                   " to ChinaJciSoybeanCrushMarginsD table.\n" + "\r\n");
                 }
             }
             string url5 = baseUrl + "?JCIKEY=ZLYKCRB&JCIPASSWORD=HUIYI68751628&RE_DATE=" + reDate;
             str = GetRequestStr(url5).Replace("\r", "").Replace("\n", "").Replace("\t", "");
             str = str.Substring(0, str.IndexOf("<!DOCTYPE html>", System.StringComparison.Ordinal));
             if (!string.IsNullOrEmpty(str))
             {
                 if (str == "Error")
                 {
                     sbSync.Append(" no synchronized from " + url5 + " to ChinaJciPalmOilStocksDaily table.\n" +
                                   "\r\n");
                 }
                 else
                 {
                     var list =
                         ((List<ChinaJciPalmOilStocksDaily>)JsonToObject(str, new List<ChinaJciPalmOilStocksDaily>())).Distinct().ToList();
                     InsertChinaJciPalmOilStocksDaily(list);
                     sbSync.Append(list.Count + " rows have been synchronized from " + url5 +
                                   " to ChinaJciPalmOilStocksDaily table.\n" + "\r\n");
                 }
             }
             string url6 = baseUrl + "?JCIKEY=XMCBGS&JCIPASSWORD=HUIYI68751628&RE_DATE=" + reDate;
             str = GetRequestStr(url6).Replace("\r", "").Replace("\n", "").Replace("\t", "");
             str = str.Substring(0, str.IndexOf("<!DOCTYPE html>", System.StringComparison.Ordinal));
             if (!string.IsNullOrEmpty(str))
             {
                 if (str == "Error")
                 {
                     sbSync.Append(" no synchronized from " + url6 +
                                   " to ChinaJciWheatCostEstimateDai table.\n" +
                                   "\r\n");
                 }
                 else
                 {
                     var list =
                         ((List<ChinaJciWheatCostEstimateDaily>)
                             JsonToObject(str, new List<ChinaJciWheatCostEstimateDaily>())).Distinct().ToList();
                     InsertChinaJciWheatCostEstimateDaily(list);
                     sbSync.Append(list.Count + " rows have been synchronized from " + url6 +
                                   " to ChinaJciWheatCostEstimateDai table.\n" + "\r\n");
                 }
             }
             string url7 = baseUrl + "?JCIKEY=YMCBGS&JCIPASSWORD=HUIYI68751628&RE_DATE=" + reDate;
             str = GetRequestStr(url7).Replace("\r", "").Replace("\n", "").Replace("\t", "");
             str = str.Substring(0, str.IndexOf("<!DOCTYPE html>", System.StringComparison.Ordinal));
             if (!string.IsNullOrEmpty(str))
             {
                 if (str == "Error")
                 {
                     sbSync.Append(" no synchronized from " + url7 +
                                   " to ChinaJciCornCostEstimateDail table.\n" +
                                   "\r\n");
                 }
                 else
                 {
                     var list =
                         ((List<ChinaJciCornCostEstimateDaily>)
                             JsonToObject(str, new List<ChinaJciCornCostEstimateDaily>())).Distinct().ToList();
                     InsertChinaJciCornCostEstimateDaily(list);
                     sbSync.Append(list.Count + " rows have been synchronized from " + url7 +
                                   " to ChinaJciCornCostEstimateDail table.\n" + "\r\n");
                 }
             }
             
        }
        string GetRequestStr(string url)
        {
            WebRequest request = WebRequest.Create(url);
            //var proxy = new WebProxy("164.179.108.25", 8118);
            //request.Proxy = proxy;
            WebResponse response = request.GetResponse();
            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            Stream dataStream = response.GetResponseStream();
            if (dataStream == null)
            {
                return "";
            }
            var reader = new StreamReader(dataStream, Encoding.UTF8);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            response.Close();
            var str = ConvertUnicodeStringToChinese(responseFromServer);
            //Console.WriteLine(str);
            return str;
        }
        // 从一个Json串生成对象信息
        public object JsonToObject(string jsonString, object obj)
        {
            var serializer = new DataContractJsonSerializer(obj.GetType());
            var mStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            return serializer.ReadObject(mStream);
        }
        public string ConvertUnicodeStringToChinese(string unicodeString)
        {
            if (string.IsNullOrEmpty(unicodeString))
                return string.Empty;

            string outStr = unicodeString;

            var re = new Regex("\\\\u[0123456789abcdef]{4}", RegexOptions.IgnoreCase);
            var mc = re.Matches(unicodeString);
            return mc.Cast<Match>().Aggregate(outStr, (current, ma) => current.Replace(ma.Value, ConverUnicodeStringToChar(ma.Value).ToString(CultureInfo.InvariantCulture)));
        }

        private char ConverUnicodeStringToChar(string str)
        {
            var outStr = (char)int.Parse(str.Remove(0, 2), NumberStyles.HexNumber);
            return outStr;
        }


        public void InsertChinaJciInternalPrice(List<ChinaJciInternalPrice> list)
        {
            var dt = new DataTable();
            dt.Columns.Add("pro_name");
            dt.Columns.Add("price");
            dt.Columns.Add("grade");
            dt.Columns.Add("mope");
            dt.Columns.Add("cdarea");
            dt.Columns.Add("sbarea");
            dt.Columns.Add("remark");
            dt.Columns.Add("re_date");
            dt.Columns.Add("mykey");
            foreach (var variable in list)
            {
                var dr = dt.NewRow();
                dr["pro_name"] = variable.pro_name;
                dr["price"] = variable.price;
                dr["grade"] = variable.grade;
                dr["mope"] = variable.mope;
                dr["cdarea"] = variable.cdarea;
                dr["sbarea"] = variable.sbarea;
                dr["remark"] = variable.remark;
                dr["re_date"] = variable.re_date;
                dr["mykey"] = variable.mykey;
                dt.Rows.Add(dr);
            }
            if (list.Count == 0)
            {
                return;
            }
            ExecuteInsertSql("ChinaJciInternalPrice", dt, list.Min(re => re.re_date), dt.Columns.Count, true);
        }
        public void InsertChinaJciImportPrice(List<ChinaJciImportPrice> list)
        {
            var dt = new DataTable();
            dt.Columns.Add("country");
            dt.Columns.Add("pro_name");
            dt.Columns.Add("price");
            dt.Columns.Add("cnf");
            dt.Columns.Add("grade");
            dt.Columns.Add("rmb");
            dt.Columns.Add("most");
            dt.Columns.Add("pack");
            dt.Columns.Add("ship");
            dt.Columns.Add("re_date");
            dt.Columns.Add("fare");
            dt.Columns.Add("lme");
            dt.Columns.Add("rate");
            dt.Columns.Add("duty");
            dt.Columns.Add("tax");
            dt.Columns.Add("port");
            dt.Columns.Add("jrate");
            dt.Columns.Add("mykey");
            foreach (var variable in list)
            {
                var dr = dt.NewRow();
                dr["country"] = variable.country;
                dr["pro_name"] = variable.pro_name;
                dr["price"] = variable.price;
                dr["cnf"] = variable.cnf;
                dr["grade"] = variable.grade;
                dr["rmb"] = variable.rmb;
                dr["most"] = variable.most;
                dr["pack"] = variable.pack;
                dr["ship"] = variable.ship;
                dr["re_date"] = variable.re_date;
                dr["fare"] = variable.fare;
                dr["lme"] = variable.lme;
                dr["rate"] = variable.rate;
                dr["duty"] = variable.duty;
                dr["tax"] = variable.tax;
                dr["port"] = variable.port;
                dr["jrate"] = variable.jrate;
                dr["mykey"] = variable.mykey;
                dt.Rows.Add(dr);
            }
            if (list.Count == 0)
            {
                return;
            }
            ExecuteInsertSql("ChinaJciImportPrice", dt, list.Min(re => re.re_date), dt.Columns.Count,true);
        }
        public void InsertChinaJciSoybeanStocksDaily(List<ChinaJciSoybeanStocksDaily> list)
        {
            var dt = new DataTable();
            dt.Columns.Add("area");
            dt.Columns.Add("stock");
            dt.Columns.Add("re_date");
            dt.Columns.Add("mykey");
            foreach (var variable in list)
            {
                var dr = dt.NewRow();
                dr["area"] = variable.area;
                dr["stock"] = variable.stock;
                dr["re_date"] = variable.re_date;
                dr["mykey"] = variable.mykey;
                dt.Rows.Add(dr);
            }
            if (list.Count == 0)
            {
                return;
            }
            ExecuteInsertSql("ChinaJciSoybeanStocksDaily", dt, list.Min(re => re.re_date), dt.Columns.Count);
        }
        public void InsertChinaJciSoybeanCrushMarginsDaily(List<ChinaJciSoybeanCrushMarginsDaily> list)
        {
            var dt = new DataTable();
            dt.Columns.Add("area");
            dt.Columns.Add("sb");
            dt.Columns.Add("sm");
            dt.Columns.Add("so");
            dt.Columns.Add("profit");
            dt.Columns.Add("smp");
            dt.Columns.Add("re_date");
            dt.Columns.Add("mykey");
            foreach (var variable in list)
            {
                var dr = dt.NewRow();
                dr["area"] = variable.area;
                dr["sb"] = variable.sb;
                dr["sm"] = variable.sm;
                dr["so"] = variable.so;
                dr["profit"] = variable.profit;
                dr["smp"] = variable.smp;
                dr["re_date"] = variable.re_date;
                dr["mykey"] = variable.mykey;
                dt.Rows.Add(dr);
            }
            if (list.Count == 0)
            {
                return;
            }
            ExecuteInsertSql("ChinaJciSoybeanCrushMarginsD", dt, list.Min(re => re.re_date), dt.Columns.Count);
        }
        public void InsertChinaJciPalmOilStocksDaily(List<ChinaJciPalmOilStocksDaily> list)
        {
            var dt = new DataTable();
            dt.Columns.Add("area");
            dt.Columns.Add("stock");
            dt.Columns.Add("re_date");
            dt.Columns.Add("mykey");
            foreach (var variable in list)
            {
                var dr = dt.NewRow();
                dr["area"] = variable.area;
                dr["stock"] = variable.stock;
                dr["re_date"] = variable.re_date;
                dr["mykey"] = variable.mykey;
                dt.Rows.Add(dr);
            }
            if (list.Count == 0)
            {
                return;
            }
            ExecuteInsertSql("ChinaJciPalmOilStocksDaily", dt, list.Min(re => re.re_date), dt.Columns.Count);
        }
        public void InsertChinaJciWheatCostEstimateDaily(List<ChinaJciWheatCostEstimateDaily> list)
        {
            var dt = new DataTable();
            dt.Columns.Add("COUNTRY");
            dt.Columns.Add("PRO_NAME");
            dt.Columns.Add("BASIS");
            dt.Columns.Add("LME");
            dt.Columns.Add("CBOT");
            dt.Columns.Add("CNF");
            dt.Columns.Add("TAX");
            dt.Columns.Add("CHTAX");
            dt.Columns.Add("FREIGHT");
            dt.Columns.Add("RATE");
            dt.Columns.Add("RE_DATE");
            dt.Columns.Add("LASTCHOOSE");
            dt.Columns.Add("MYKEY");
            foreach (var variable in list)
            {
                var dr = dt.NewRow();
                dr["COUNTRY"] = variable.COUNTRY;
                dr["PRO_NAME"] = variable.PRO_NAME;
                dr["BASIS"] = variable.BASIS;
                dr["LME"] = variable.LME;
                dr["CBOT"] = variable.CBOT;
                dr["CNF"] = variable.CNF;
                dr["TAX"] = variable.TAX;
                dr["CHTAX"] = variable.CHTAX;
                dr["FREIGHT"] = variable.FREIGHT;
                dr["RATE"] = variable.RATE;
                dr["RE_DATE"] = variable.RE_DATE;
                dr["LASTCHOOSE"] = variable.LASTCHOOSE;
                dr["MYKEY"] = variable.MYKEY;
                dt.Rows.Add(dr);
            }
            if (list.Count == 0)
            {
                return;
            }
            ExecuteInsertSql("ChinaJciWheatCostEstimateDai", dt, list.Min(re => re.RE_DATE), dt.Columns.Count);
        }
        public void InsertChinaJciCornCostEstimateDaily(List<ChinaJciCornCostEstimateDaily> list)
        {
            var dt = new DataTable();
            dt.Columns.Add("COUNTRY");
            dt.Columns.Add("MONTH");
            dt.Columns.Add("BASIS");
            dt.Columns.Add("LME");
            dt.Columns.Add("CBOT");
            dt.Columns.Add("CNF");
            dt.Columns.Add("DUTY");
            dt.Columns.Add("LASTDUTY");
            dt.Columns.Add("FREIGHT");
            dt.Columns.Add("RATE");
            dt.Columns.Add("RE_DATE");
            dt.Columns.Add("FREEIMPORT");
            dt.Columns.Add("MYKEY");
            foreach (var variable in list)
            {
                var dr = dt.NewRow();
                dr["COUNTRY"] = variable.COUNTRY;
                dr["MONTH"] = variable.MONTH;
                dr["BASIS"] = variable.BASIS;
                dr["LME"] = variable.LME;
                dr["CBOT"] = variable.CBOT;
                dr["CNF"] = variable.CNF;
                dr["DUTY"] = variable.DUTY;
                dr["LASTDUTY"] = variable.LASTDUTY;
                dr["FREIGHT"] = variable.FREIGHT;
                dr["RATE"] = variable.RATE;
                dr["RE_DATE"] = variable.RE_DATE;
                dr["FREEIMPORT"] = variable.FREEIMPORT;
                dr["MYKEY"] = variable.MYKEY;
                dt.Rows.Add(dr);
            }
            if (list.Count == 0)
            {
                return;
            }
            ExecuteInsertSql("ChinaJciCornCostEstimateDail", dt, list.Min(re => re.RE_DATE), dt.Columns.Count);
        }

        public void ExecuteInsertSql(string tableName,DataTable tableData,string minReDate,int paraCount,bool isHasHistoryValue=false)
        {
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                var cmd = new OracleCommand("delete " + tableName + " where RE_DATE>='" + minReDate + "'", con);
                cmd.ExecuteNonQuery();
                con.Close();
            }
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                var cmdText = "INSERT INTO " + tableName + " VALUES( ";
                for (var i = 0; i < paraCount; i++)
                {
                    cmdText += ":v" + i + ",";
                }
                if (isHasHistoryValue)
                {
                    cmdText += "sysdate,null)";
                }
                else
                {
                    cmdText += "sysdate)";
                }
                var cmd = new OracleCommand(cmdText, con);

                for (var j = 0; j < tableData.Rows.Count; j++)
                {
                    cmd.Parameters.Clear();
                    for (var i = 0; i < paraCount; i++)
                    {
                        var par = new OracleParameter("v" + i, OracleDbType.NVarchar2)
                        {
                            Direction = ParameterDirection.Input,
                            Value = tableData.Rows[j][i].ToString()
                        };
                        cmd.Parameters.Add(par);
                    }
                    cmd.ExecuteNonQuery();
                }
                con.Close();
            }
        }

    }
}
