using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;
using OracleDataAdapter = Oracle.ManagedDataAccess.Client.OracleDataAdapter;
using OracleParameter = Oracle.ManagedDataAccess.Client.OracleParameter;
namespace CNE.Scheduler.Extension
{
    public class SmmShNew
    {
        string connstr = ConfigurationManager.AppSettings["CnECon"].ToString();
        public string GetUrlString(StringBuilder sb)
        {

            string url = ConfigurationManager.AppSettings["smmurl"].ToString();

            try
            {
                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);

                var str = reader.ReadToEnd();
                reader.Close();
                return str.Substring(str.IndexOf("<body>") + 8).Replace("\r\n", "");
            }
            catch (Exception e)
            {
                sb.AppendFormat("[" + e.Message + "{0}]", DateTime.Now.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss"));
                return "";
            }

        }

        public string GetTableString(string type, string input)
        {
            string t = "<h1>" + type + @"</h1>\n*\s*?<table[^>]+>([\w\s\S]+?)</table>";
            Regex reg = new Regex(t);
            if (reg.IsMatch(input))
            {
                return reg.Match(input).Value;
            }
            else
            {
                return "";
            }
        }
        //metals_smm_shanghai_cn
        public DataTable CreateDataTable(string type)
        {
            DataTable tb = new DataTable();
            switch (type)
            {
                case "SMMPrices_路透":
                    tb.TableName = "metals_smm_shanghai";
                    tb.Columns.Add("id");
                    tb.Columns.Add("code");
                    tb.Columns.Add("productName");
                    tb.Columns.Add("unit");
                    tb.Columns.Add("specification");
                    tb.Columns.Add("grade");
                    tb.Columns.Add("brand");
                    tb.Columns.Add("locationOfSale");
                    tb.Columns.Add("locationOfProduction");
                    tb.Columns.Add("producer");
                    tb.Columns.Add("lowestPrice");
                    tb.Columns.Add("highestPrice");
                    tb.Columns.Add("updateDate");
                    tb.Columns.Add("FETCHTIME");
                    tb.Columns.Add("Lanuage");
                    break;
                case "SMMPrices_toThomsonReuters":
                    tb.TableName = "metals_smm_shanghai";
                     tb.Columns.Add("id");
                     tb.Columns.Add("code");
                    tb.Columns.Add("productName");
                    tb.Columns.Add("unit");
                    tb.Columns.Add("specification");
                    tb.Columns.Add("grade");
                    tb.Columns.Add("brand");
                    tb.Columns.Add("locationOfSale");
                    tb.Columns.Add("locationOfProduction");
                    tb.Columns.Add("producer");
                    tb.Columns.Add("lowestPrice");
                    tb.Columns.Add("highestPrice");
                    tb.Columns.Add("updateDate");
                    tb.Columns.Add("FETCHTIME");
                    tb.Columns.Add("Lanuage");
                    break;
                case "SMM行业数据":
                    //id, datatype, dataname, unit, figure, updateDate, fetchTime
                    tb.TableName = "metals_smm_industry";
                     tb.Columns.Add("id");
                     tb.Columns.Add("code");
                    tb.Columns.Add("datatype");
                    tb.Columns.Add("dataname");
                    tb.Columns.Add("priceDate");
                    tb.Columns.Add("unit");
                    tb.Columns.Add("figure");
                    tb.Columns.Add("updateDate");
                    tb.Columns.Add("fetchTime");
                    tb.Columns.Add("Lanuage");
                    break;
                case "SMM data":
                    tb.TableName = "metals_smm_industry";
                    tb.Columns.Add("id");
                    tb.Columns.Add("code");
                    tb.Columns.Add("datatype");
                    tb.Columns.Add("dataname");
                    tb.Columns.Add("priceDate");
                    tb.Columns.Add("unit");
                    tb.Columns.Add("figure");
                    tb.Columns.Add("updateDate");
                    tb.Columns.Add("fetchTime");
                    tb.Columns.Add("Lanuage");
                    break;
                default:
                    break;

            }
            return tb;
        }
    
        public void FillDataTable_Product(DataTable tb, string str,int language)
        {
            //s == "SMMPrices_路透" || s == "SMMPrices_toThomsonReuters"
            Regex reg = new Regex(@"<tr>([\w\s\S]+?)</tr>");
            OracleConnection conn = new OracleConnection(connstr);
            try
            {
                if (reg.IsMatch(str))
                {
                    MatchCollection coll = reg.Matches(str);
                    for (int i = 1; i < coll.Count; i++)
                    {
                        string tr = coll[i].Value;
                        //.Replace(@"</td>\s+", "@").
                        //.Replace(@"<tr>\s+", "")

                        tr = tr.Replace("<td class=\"spec\">", "@").Replace("<td>", "@").Replace("</tr>", "");
                        tr = new Regex(@"</td>\s+").Replace(tr, "@");
                        tr = new Regex(@"<tr>\s+").Replace(tr, "");
                        string[] strArray = tr.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
                        //  id, productName, unit, specification, grade, brand, locationOfSale, locationOfProduction, producer, lowestPrice, highestPrice, updateDate, updateDateTime
                        DataRow dr = tb.NewRow();
                        //string pname=System .Text .Encoding .UTF8.GetString (System.Text.Encoding.GetEncoding("gb2312").GetBytes());
                        dr["id"] = strArray[0].Replace("&nbsp;", "");
                        string code = getCode(0, i, conn);
                        if (string.IsNullOrEmpty(code))
                        {
                            code = strArray[0].Replace("&nbsp;", "");
                        }
                        dr["code"] = code;
                        dr["productName"] = strArray[1].Replace("&nbsp;", "");
                        dr["unit"] = strArray[2].Replace("&nbsp;", "");
                        dr["specification"] = strArray[3].Replace("&nbsp;", "");
                        dr["grade"] = strArray[4].Replace("&nbsp;", "");
                        dr["brand"] = strArray[5].Replace("&nbsp;", "");
                        dr["locationOfSale"] = strArray[6].Replace("&nbsp;", "");
                        dr["locationOfProduction"] = strArray[7].Replace("&nbsp;", "");
                        dr["producer"] = strArray[8].Replace("&nbsp;", "");
                        dr["lowestPrice"] = strArray[9].Replace("&nbsp;", "");
                        dr["highestPrice"] = strArray[10].Replace("&nbsp;", "");
                        dr["updateDate"] = strArray[11].Replace("&nbsp;", "").Replace("/", "-");
                        //dr["updateDateTime"] = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
                        dr["FETCHTIME"] = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt");
                        dr["Lanuage"] = language;
                        //if (Convert.ToDateTime(dr["updateDate"]) != DateTime.Now.Date)
                        //{
                        //    continue;
                        //}
                        tb.Rows.Add(dr);
                    }
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                conn.Close();
            }


        }
        public string getCode(int type,int order,OracleConnection conn)
        {
            string sql = "select * from SmmCode where TYPEC=" + type + "and ORDERC=" + order;
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {

                OracleDataAdapter sda = new OracleDataAdapter(cmd);
                DataTable tb = new DataTable();
                sda.Fill(tb );
                if (tb != null && tb.Rows.Count == 1)
                {
                    return tb.Rows[0]["CODE"].ToString();
                }
                else
                {
                    return "";
                }
                

            }
        }
        public void FillData(DataTable tb, string str,int language)
        {
            Regex reg = new Regex(@"<tr>([\w\s\S]+?)</tr>");
            OracleConnection conn = new OracleConnection(connstr);
            try
            {
                if (reg.IsMatch(str))
                {
                    MatchCollection coll = reg.Matches(str);
                    for (int i = 1; i < coll.Count; i++)
                    {
                        string tr = coll[i].Value;
                        tr = tr.Replace("<td class=\"spec\">", "@").Replace("<td>", "@").Replace("</tr>", "");

                        tr = new Regex(@"</td>\s+").Replace(tr, "@");
                        tr = new Regex(@"<tr>\s+").Replace(tr, "");


                        string[] strArray = tr.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
                        // datatype, dataname, unit, figure, updateDate, fetchTime
                        DataRow dr = tb.NewRow();
                        dr["id"] = strArray[0].Replace("&nbsp;", "");
                        string code=getCode(1, i, conn);
                        if (string.IsNullOrEmpty(code))
                        {
                            code = strArray[0].Replace("&nbsp;", "");
                        }
                        dr["code"] = code;
                        dr["datatype"] = strArray[1].Replace("&nbsp;", "");
                        dr["dataname"] = strArray[2].Replace("&nbsp;", "");
                        dr["priceDate"] = strArray[3].Replace("&nbsp;", "");
                        dr["unit"] = strArray[4].Replace("&nbsp;", "");
                        dr["figure"] = strArray[5].Replace("&nbsp;", "");
                        dr["updateDate"] = strArray[6].Replace("&nbsp;", "").Replace("/", "-");
                        //dr["fetchTime"] = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
                        dr["fetchTime"] = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt");
                        dr["Lanuage"] = language;
                        //if (Convert.ToDateTime (dr["updateDate"])!= DateTime.Now.Date )
                        //{
                        //    continue;
                        //}

                        tb.Rows.Add(dr);
                    }
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                conn.Close();
            }
        }

        private string CreateUpdateSql(DataTable tb, DataRow dr, string strWhere)
        {
            StringBuilder sb = new StringBuilder("update " + tb.TableName + " set ");
            for (int i = 0; i < tb.Columns.Count; i++)
            {
                sb.Append(tb.Columns[i].ColumnName + "='" + dr[tb.Columns[i].ColumnName].ToString ().Replace ("'","''") + "'  ");
                if (i != tb.Columns.Count - 1)
                {
                    sb.Append(",");
                }
            }
            sb.Append(strWhere);
            return sb.ToString();

        }
        private string CreateInsertSql(DataTable tb, DataRow dr)
        {
            StringBuilder sb = new StringBuilder("insert into " + tb.TableName + "  (");
            string values = string.Empty;
            for (int i = 0; i < tb.Columns.Count; i++)
            {
                sb.Append(tb.Columns[i].ColumnName);
                values += "'" + dr[tb.Columns[i].ColumnName].ToString ().Replace ("'","''") + "'";
                if (i != tb.Columns.Count - 1)
                {
                    sb.Append(",");
                    values += ",";
                }
            }
            sb.Append(") values(");
            sb.Append(values);
            sb.Append(")");
            return sb.ToString();

        }
        private string CreateFilter(string[] filters, DataRow dr)
        {
            StringBuilder sb = new StringBuilder("  where ");
            for (int i = 0; i < filters.Length; i++)
            {
                sb.Append(filters[i] + "='" + dr[filters[i]] + "'");
                if (i != (filters.Length - 1))
                {
                    sb.Append("  and  ");
                }
            }
            return sb.ToString();
        }
        public void ImportData(DataTable tb, string[] filters, StringBuilder sb)
        {
            int updateRows = 0, insertRows = 0;
            using (OracleConnection conn = new OracleConnection(connstr))
            {
                conn.Open();
                using (OracleCommand cmd = new OracleCommand())
                {
                    cmd.Connection = conn;
                    foreach (DataRow dr in tb.Rows)
                    {

                        string strWhere = CreateFilter(filters, dr);
                        string sqlExists = "select * from " + tb.TableName + strWhere;
                        cmd.CommandText = sqlExists;
                        OracleDataAdapter sda = new OracleDataAdapter(cmd);
                        DataTable t = new DataTable();
                        sda.Fill(t);
                        if (t != null && t.Rows.Count == 1)
                        {
                            //更新数据；
                            string updatesql = CreateUpdateSql(tb, dr, strWhere);
                            cmd.CommandText = updatesql;
                            cmd.ExecuteNonQuery();
                            updateRows++;
                        }
                        else
                        {
                            //插入数据；
                            string updatesql = CreateInsertSql(tb, dr);
                            cmd.CommandText = updatesql;
                            cmd.ExecuteNonQuery();
                            insertRows++;
                        }
                    }
                }
                conn.Close();
                sb.Append(" update:" + updateRows + " insert:" + insertRows + "\r\n");
            }

        }
        public void InsertDataBase(string[] strs, StringBuilder sb)
        {
            try
            {
                string urlString = GetUrlString(sb);
                foreach (string s in strs)
                {
                    string tableStr = GetTableString(s, urlString);
                    DataTable tb = CreateDataTable(s);
                    if (s == "SMMPrices_路透" || s == "SMMPrices_toThomsonReuters")
                    {
                        if (s == "SMMPrices_路透")
                        {
                            FillDataTable_Product(tb, tableStr,0);//0中文
                            string[] strWhere = { "code", "updateDate", "Lanuage" };
                            sb.Append("Table METALS_SMM_SHANGHAI & Langunge=0 :");
                            ImportData(tb, strWhere,sb);
                        }
                        else if (s == "SMMPrices_toThomsonReuters")
                        {
                            FillDataTable_Product(tb, tableStr,1);//1英文
                            string[] strWhere = { "code", "updateDate", "Lanuage" };
                            sb.Append("Table METALS_SMM_SHANGHAI & Langunge=1 :");
                            ImportData(tb, strWhere, sb);
                        }
                    }
                    else
                    {
                        //SMM行业数据", "SMM data" 
                        if (s == "SMM行业数据")
                        {
                            FillData(tb, tableStr,0);
                            string[] strWhere = { "code", "priceDate", "Lanuage" };
                            sb.Append("Table METALS_SMM_INDUSTRY & Langunge=0 :");
                            ImportData(tb, strWhere,sb);
                        }
                        else if (s == "SMM data")
                        {
                            FillData(tb, tableStr,1);
                            string[] strWhere = { "code", "priceDate", "Lanuage" };
                            sb.Append("Table METALS_SMM_INDUSTRY & Langunge=1 :");
                            ImportData(tb, strWhere,sb);
                        }

                    }
                }
            }
            catch (Exception e)
            {
                sb.AppendFormat("[insert table  failed,Because:{1}]", e.Message + "[" + DateTime.Now.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss") + "]");
                throw e;
            }
        }

    }
}
