using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Data;
using System.Reflection;
using System.Configuration;
using CNE.Scheduler.Extension.Model;
namespace CNE.Scheduler.Extension
{



    public abstract class FetchDataBase
    {
        public string GetRemoteDataFromUrl(string url, StringBuilder sb)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                //longProxy  longPort
                //WebProxy proxy = new WebProxy(ConfigurationManager.AppSettings["longProxy"].ToString(), Convert.ToInt32(ConfigurationManager.AppSettings["longPort"].ToString()));
                //request.Proxy = proxy;
                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);

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
 
        public PackageProduct<P> ConvertString2Object<P>(string jsonString, StringBuilder logMsg)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                logMsg.AppendFormat(" [data not exists：{0}]", DateTime.Now.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss"));
                PackageProduct<P> p = new PackageProduct<P>();
                p.data = new List<P>();
                return p;
            }
            PackageProduct<P> instance = Activator.CreateInstance<PackageProduct<P>>();
            string[] strTemp = jsonString.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
            if (strTemp.Length != 3)
            {
                logMsg.AppendFormat("[The data will not be updated from url:{0}]", DateTime.Now.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss"));
                PackageProduct<P> p = new PackageProduct<P>();
                p.data = new List<P>();
                return p;



                //throw new Exception("数据字符串不存在");
            }
            string code = strTemp[0].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[0].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[1].Replace("\"", "");

            string msg = strTemp[0].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[1].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[1].Replace("\"", "");
            string data = strTemp[1];
            int intCode;
            instance.code = string.IsNullOrEmpty(code) ? 0 : (int.TryParse(code, out intCode) ? intCode : 0);
            instance.msg = msg;
            data = data.Replace("{", "@").Replace("},", "@").Replace("}", "@");
            string[] dataArray = data.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            if (dataArray.Length > 0)
            {
                instance.data = new List<P>();
            }
            //创建JSON正则表达式；
            P item = default(P);

            int iy = dataArray.Length;
            for (int i = 0; i < dataArray.Length; i++)
            {
                item = Activator.CreateInstance<P>();
                string[] jsonData = dataArray[i].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                ///////////////////
                int first = 0;
                string buf = string.Empty;
                /////////////////
                for (int j = 0; j < jsonData.Length; j++)
                {



                    if (jsonData[j].IndexOf(":") < 0)
                    {


                        /////////
                        if (first == 0)
                        {
                            first = j - 1;
                        }
                        buf += "," + jsonData[j];

                        /////////////////

                        //jsonData[j - 1] += "," + jsonData[j];
                        jsonData[j] = "";
                    }

                }
                jsonData[first] += buf;
                for (int j = 0; j < jsonData.Length; j++)
                {
                    if (string.IsNullOrEmpty(jsonData[j]))
                    {
                        continue;
                    }

                    string propertyName = jsonData[j].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[0].Replace("\"", "");
                    string pValue = string.Empty;
                    string[] updateTime = jsonData[j].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (updateTime.Length == 2)
                    {
                        pValue = updateTime[1].Replace("\"", "");
                    }
                    else
                    {
                        for (int k = 1; k < updateTime.Length; k++)
                        {
                            pValue += updateTime[k].Replace("\"", "") + ":";
                        }
                        pValue = pValue.Substring(0, pValue.Length - 1);
                    }
                    var pros = item.GetType().GetProperties();
                    for (int k = 0; k < pros.Length; k++)
                    {
                        if (pros[k].Name == propertyName.Trim())
                        {
                            if (pValue == "null")
                            {
                                pros[k].SetValue(item, null, null);
                            }
                            else
                            {
                                pros[k].SetValue(item, Convert.ChangeType(pValue, pros[k].PropertyType), null);
                            }
                        }
                    }
                }
                instance.data.Add(item);
            }


            return instance;
        }

        private void DataTableAddCoumns_GuoNei(DataTable tb)
        {
            tb.Columns.Add("id");
            tb.Columns.Add("productName");
            tb.Columns.Add("modelName");
            tb.Columns.Add("areaName");
            tb.Columns.Add("unit");
            tb.Columns.Add("memo");
            tb.Columns.Add("marketName");
            tb.Columns.Add("manufactureName");
        }
        private void DataTableAddColumns_GuoJi(DataTable tb)
        {
            tb.Columns.Add("id");
            tb.Columns.Add("productName");
            tb.Columns.Add("modelName");
            tb.Columns.Add("areaName");
            tb.Columns.Add("unit");
            tb.Columns.Add("memo");
        }
        private void DataTableAddColumns_ChuChang(DataTable tb)
        {
            tb.Columns.Add("id");
            tb.Columns.Add("productName");
            tb.Columns.Add("modelName");
            tb.Columns.Add("areaName");
            tb.Columns.Add("unit");
            tb.Columns.Add("memo");
            tb.Columns.Add("manufactureName");
        }
        private void DataTableAddColumns_Oil(DataTable tb)
        {
            tb.Columns.Add("id");
            tb.Columns.Add("productName");
            tb.Columns.Add("modelName");
            tb.Columns.Add("areaName");
            tb.Columns.Add("unit");
            tb.Columns.Add("memo");
            tb.Columns.Add("marketName");
        }
        private void DataTableAddColumns_ChuPrice(DataTable tb)
        {
            tb.Columns.Add("id");
            tb.Columns.Add("priceDate");
            tb.Columns.Add("memo");
            tb.Columns.Add("price");
            tb.Columns.Add("updateDate");
        }
        private void DataTableAddColumns_OilPrice(DataTable tb)
        {
            tb.Columns.Add("id");
            tb.Columns.Add("priceDate");
            tb.Columns.Add("memo");
            tb.Columns.Add("price");
            tb.Columns.Add("zsyPrice");
            tb.Columns.Add("zshPrice");
            tb.Columns.Add("updateDate");
        }
        private void DataTableAddCoulmns_GuoJiPrice(DataTable tb)
        {
            tb.Columns.Add("id");
            tb.Columns.Add("priceDate");
            tb.Columns.Add("memo");
            tb.Columns.Add("price");
            tb.Columns.Add("lowPrice");
            tb.Columns.Add("highPrice");
            tb.Columns.Add("priceType");
            tb.Columns.Add("updateDate");
            tb.PrimaryKey = new DataColumn[] {tb.Columns["id"], 
                                         tb.Columns["priceDate"]};
        }
        private void DataTableAddColumn_GuoNeiPrice(DataTable tb)
        {
            tb.Columns.Add("id");
            tb.Columns.Add("priceDate");
            tb.Columns.Add("memo");
            tb.Columns.Add("price");
            tb.Columns.Add("lowPrice");
            tb.Columns.Add("highPrice");
            tb.Columns.Add("updateDate");
        }
        public DataTable CreateDataTableSchema(Type t)
        {
            DataTable tb = new DataTable();

            if (t == typeof(GuoNeiProduct))
            {
                DataTableAddCoumns_GuoNei(tb);
                tb.TableName = t.Name;
            }
            else if (t == typeof(GuoJiProduct))
            {
                DataTableAddColumns_GuoJi(tb);
                tb.TableName = t.Name;
            }
            else if (t == typeof(ChuChangProduct))
            {
                DataTableAddColumns_ChuChang(tb);
                tb.TableName = t.Name;
            }
            else if (t == typeof(OilProduct))
            {
                DataTableAddColumns_Oil(tb);
                tb.TableName = t.Name;
            }
            else if (t == typeof(ChuChangPrice))
            {
                DataTableAddColumns_ChuPrice(tb);
                tb.TableName = t.Name;
            }
            else if (t == typeof(OilPrice))
            {
                DataTableAddColumns_OilPrice(tb);
                tb.TableName = t.Name;
            }
            else if (t == typeof(GuoJiPrice))
            {
                DataTableAddCoulmns_GuoJiPrice(tb);
                tb.TableName = t.Name;
            }
            else if (t == typeof(GuoNeiPrice))
            {
                DataTableAddColumn_GuoNeiPrice(tb);
                tb.TableName = t.Name;
            }
            return tb;
        }
        public DataTable FillDataFromList<T>(DataTable tb, List<T> list)
        {
            DataColumn[] cols = tb.PrimaryKey;
            foreach (T entity in list)
            {
                DataRow dr = tb.NewRow();
                Type tp = typeof(T);
                PropertyInfo[] pinfos = tp.GetProperties();
                for (int i = 0; i < pinfos.Length; i++)
                {
                    string columnName = pinfos[i].Name;

                    Object o = pinfos[i].GetValue(entity, null);
                    dr[columnName] = o;

                }
                if (tb.PrimaryKey.Length!=0)
                {
                    object[] objs = new object[cols.Length];
                    for (int i = 0; i < cols.Length; i++)
                    {
                        objs[i] = dr[cols[i].ColumnName];
                    }
                    DataRow drByPrimary = tb.Rows.Find(objs);
                    if (drByPrimary == null)
                    {
                        tb.Rows.Add(dr);
                    }
                }
                else
                {
                    tb.Rows.Add(dr);
                }

            }
            return tb;
        }

        public abstract void FetchAndFill(StringBuilder sb);

    }
}
