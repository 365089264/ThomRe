
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;

namespace CNE.Scheduler.Extension
{
    public class FanYaExponent
    {
        
        private const string StockUrl = "http://fyme.cn/getmarket/market_kucun.php";
        private const string ExponentUrl = "http://fyme.cn/getmarket/market_zhishu.php";
        private readonly SqlConnection _connectiononn = new SqlConnection("server=10.35.63.144;database=cne;uid=sa;password=p@ssw0rd");
        private string GetUrlString(string url)
        {
            string result = string.Empty;
            var request = WebRequest.Create(url);
            var response = request.GetResponse();
            var stream = response.GetResponseStream();
            if (stream == null)
            {
                return result;
            }
            using (var streamReader = new StreamReader(stream, Encoding.GetEncoding("gbk")))
            {
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        private DataTable CreateTabSchema<T>()
        {
            var tb = new DataTable();
            Type tableSchema = typeof(T);
            foreach (var i in tableSchema.GetProperties())
            {
                tb.Columns.Add(i.Name);
            }
            tb.TableName = tableSchema.Name;
            return tb;

        }

        private string FetchFanyaData<T>(T t)
        {
            if (t.GetType().Name == "FanYaStockData")
            {
                return GetUrlString(StockUrl);
            }
            return GetUrlString(ExponentUrl);
        }
        private List<T> ToObjectList<T>(string str)
        {
            var containerList = new List<T>();
            string[] items = str.Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
            const string regexStr = @"\$(\d+\s*-\s*\d+\s*-\s*\d+)\$*(\d*)\s*-*\s*(\d*)";
            var reg = new Regex(regexStr);

            Match m = reg.Match(items[items.Length - 1]);
            string date = m.Groups[1].Value.Replace(" ", "");
            int hour = string.IsNullOrEmpty(m.Groups[2].Value) ? 0 : Convert.ToInt32(m.Groups[2].Value.Replace(" ", ""));
            int minutes = string.IsNullOrEmpty(m.Groups[3].Value) ? 0 : Convert.ToInt32(m.Groups[3].Value.Replace(" ", ""));
            var dataString = DateTime.Parse(date).AddHours(hour).AddMinutes(minutes).ToString("yyyy-MM-dd HH:mm:ss");
            for (int i = items.Length - 2; i >= 0; i--)
            {
                var tObject = Activator.CreateInstance<T>();
                String[] pValues = items[i].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                var pros = tObject.GetType().GetProperties();
                int pLength = pros.Length;
                for (var index = 1; index < pLength - 1; index++)
                {
                    pros[index].SetValue(tObject, pValues[index - 1], null);
                }
                tObject.GetType().GetProperties()[0].SetValue(tObject, Guid.NewGuid().ToString().Replace("-", ""), null);
                tObject.GetType().GetProperties()[pLength - 1].SetValue(tObject, dataString, null);
                containerList.Add(tObject);

            }
            return containerList;

        }

        private string CreateInsertSql<T>(T t)
        {
            var insertSql = new StringBuilder("insert into ");
            insertSql.Append(typeof(T).Name);
            insertSql.Append("(");
            var fields = new StringBuilder();
            var values = new StringBuilder();
            var pinfo = t.GetType().GetProperties();
            for (var i = 0; i < pinfo.Length; i++)
            {
                fields.Append(pinfo[i].Name + ",");
                string v = pinfo[i].GetValue(t, null).ToString();
                float vf;
                if (float.TryParse(v, out vf))
                {
                    values.Append(vf + ",");
                }
                else
                {
                    values.Append("N'" + v + "',");
                }
            }
            insertSql.Append(fields.ToString().Substring(0, fields.Length - 1));
            insertSql.Append(")values(");
            insertSql.Append(values.ToString().Substring(0, values.Length - 1));
            insertSql.Append(")");
            return insertSql.ToString();
        }

        private string CreateExistsSql<T>(T t)
        {
            var code = t.GetType().GetProperty("ProductCode").GetValue(t, null).ToString();
            var time = t.GetType().GetProperty("StockTime").GetValue(t, null).ToString();
            var wareName = t.GetType().GetProperty("WareHouseName") == null ? "" : t.GetType().GetProperty("WareHouseName").GetValue(t, null).ToString();
            string appender = typeof(T).Name == "FanYaStockData" ? " and WareHouseName=N'" + wareName + "'" : "";
            string sql = "declare @id int;select @id=count(*) from " + t.GetType().Name + " where ProductCode='" + code + "' and StockTime='" + time + "'" + appender + ";if(@id=0) begin select 0;end else";
            sql += " select id from " + t.GetType().Name + " where ProductCode='" + code + "' and StockTime='" + time + "'"+appender;
            return sql;
        }

        private string CreateUpdateSql<T>(T t,string Id)
        {
            var insertSql = new StringBuilder("Update  ");
            StringBuilder sb = new StringBuilder();

            insertSql.Append(typeof(T).Name);
            insertSql.Append(" set ");
            var pinfo = t.GetType().GetProperties();
            for (var i = 0; i < pinfo.Length; i++)
            {
                if (pinfo[i].Name == "Id") continue;
               
                string v = pinfo[i].GetValue(t, null).ToString();
                float vf;
                if (float.TryParse(v, out vf))
                {
                    sb.Append(pinfo[i].Name + "=" + vf + ",");
                }
                else
                {
                    sb.Append(pinfo[i].Name + "=N'" + v + "',");
                }
            }
          
            insertSql.Append(sb .ToString().Substring(0,sb .Length-1));
            insertSql.Append("where Id='" + Id+"'");
            return insertSql.ToString();
        }

        private void InsertToDataBase<T>(List<T> list)
        {
            _connectiononn.Open();
            SqlTransaction transaction = _connectiononn.BeginTransaction();
            try
            {

                string cBack = string.Empty;
                IEnumerator<T> itor = list.GetEnumerator();
                while (itor.MoveNext())
                {
                    T t = itor.Current;
                    using (var cmd = new SqlCommand(CreateExistsSql<T>(t), _connectiononn))
                    {
                        cmd.Transaction = transaction;
                       cBack= cmd.ExecuteScalar().ToString();
                    }
                    string mSql = (cBack == "0") ? CreateInsertSql<T>(t) : CreateUpdateSql<T>(t,cBack);
                    using (var cmd = new SqlCommand(mSql, _connectiononn))
                    {
                        cmd.Transaction = transaction;
                        cmd.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                _connectiononn.Close();
            }
        }

        public void ManagerStock<T>()
        {
            string str = FetchFanyaData(Activator.CreateInstance<T>());
            var list = ToObjectList<T>(str);
            InsertToDataBase<T>(list);
        }
    }
}
