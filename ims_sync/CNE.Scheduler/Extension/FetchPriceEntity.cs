using System;
using System.Text;
using System.Configuration;
using System.Data;
using System.Linq;
using CNE.Scheduler.Extension.Model;
using Oracle.ManagedDataAccess.Client;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;

namespace CNE.Scheduler.Extension
{
    public class FetchPriceEntity<T> : FetchDataBase
        where T : PriceBase
    {
        private readonly string _urlname;
        private readonly string _tablename;
        public FetchPriceEntity(string urlname, string tablename)
        {
            _urlname = urlname;
            _tablename = tablename;

        }
        public string DataUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["PriceUrl"].Replace("{type}", _urlname) + "?username=lutou&password=lutou2014data";
            }
        }
        public string DataUrlWithUnix
        {
            get
            {
                return ConfigurationManager.AppSettings["PriceUrl"].Replace("{type}", _urlname) + "?username=lutou&password=lutou2014data&modifyDate={unix}";
            }
        }

        private DateTime GetMaxUnxi()
        {
            object o;
            using (OracleConnection conn = new OracleConnection(ConfigurationManager.AppSettings["CnECon"]))
            {
                using (OracleCommand cmd = new OracleCommand("select max(updateDate) from " + _tablename, conn))
                {
                    conn.Open();
                    o = cmd.ExecuteScalar();
                    conn.Close();
                }

            }

            return DateTime.Parse(o.ToString());
        }

        public override void FetchAndFill(StringBuilder sb)
        {
            
            long maxUnix = (long)Math.Floor((GetMaxUnxi().AddHours(-8) - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds);
            string jsonData = GetRemoteDataFromUrl(DataUrlWithUnix.Replace("{unix}", maxUnix.ToString()), sb);

            PackageProduct<T> package = ConvertString2Object<T>(jsonData, sb);
            if (package.data.Count == 0)
            {
                //sb.Append(_tablename + " data Sync Rows:0 \n");
                return;
            }
            sb.Append("Source [Type: URL,  Address: " + DataUrlWithUnix.Replace("{unix}", maxUnix.ToString()) + "\n");
            if (package.code != 1)
                throw new Exception("get data failed");



            DataTable tb = CreateDataTableSchema(typeof(T));
            tb = FillDataFromList(tb, package.data);
            tb.TableName = _tablename;
            int insertCount = 0;
            int updateCount = 0;

            //OracleConnection _conn = new OracleConnection(ConfigurationManager.AppSettings["CnECon"]);
            using (var con = new OracleConnection(ConfigurationManager.AppSettings["CnECon"]))
            {
                con.Open();
                OracleCommand cmd = new OracleCommand("", con);
                foreach (DataRow dr in tb.Rows)
                {
                    string existStr = "select count(*) from " + _tablename + " where id=" + dr["id"] +
                                                     " and priceDate='" + dr["priceDate"] + "'";
                    cmd.CommandText = existStr;
                    int cmdresult = 0;
                    try
                    {

                        object obj = cmd.ExecuteScalar();
                        if (Equals(obj, null) || (Equals(obj, DBNull.Value)))
                        {
                        }
                        else
                        {
                            cmdresult = int.Parse(obj.ToString());
                        }
                    }
                    catch (OracleException e)
                    {
                        con.Close();
                        throw new Exception(e.Message);
                    }
                    string operationSql;
                    if (cmdresult == 0)
                    {
                        insertCount++;
                        operationSql = CreateInsertSql(_tablename, tb.Columns, dr);
                    }
                    else
                    {
                        updateCount++;
                        operationSql = CreateUpdateSql(_tablename, new[] { "id", "priceDate" }, tb.Columns, dr);
                    }
                    cmd.CommandText = operationSql;
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (OracleException e)
                    {
                        con.Close();
                        throw new Exception(e.Message);
                    }

                }
                con.Close();
            }
            sb.Append("Table " + _tablename + " Insert Rows:" + insertCount + ",Update Rows:" + updateCount + " \r\n");
        }

        public string CreateInsertSql(string tableName, DataColumnCollection columns, DataRow row, string sysdateColumn = "FETCHUNIX")
        {
            string insertSql = "insert into " + tableName + " (";
            string insertValues = " values (";
            foreach (DataColumn col in columns)
            {
                insertSql += col.ColumnName + ",";
                if (col.ColumnName.ToLower() == "id")
                {
                    insertValues += row[col.ColumnName] + ",";
                }
                else
                {
                    insertValues += "'" + row[col.ColumnName] + "',";
                }

            }
            insertSql += sysdateColumn + ") ";
            insertValues += "sysdate)";
            insertSql += insertValues;
            return insertSql;
        }

        public string CreateUpdateSql(string tableName, string[] primaryColumns, DataColumnCollection columns, DataRow row, string sysdateColumn = "FETCHUNIX")
        {
            string updateSql = "update " + tableName + " set " + sysdateColumn + "=sysdate";
            foreach (DataColumn col in columns)
            {
                if (!primaryColumns.Contains(col.ColumnName))
                {
                    updateSql += "," + col.ColumnName + "='" + row[col.ColumnName] + "'";
                }
            }
            updateSql += " where ";
            foreach (var primaryKey in primaryColumns)
            {
                if (primaryKey == "id")
                {
                    updateSql += columns[primaryKey].ColumnName + "=" + row[primaryKey] + " and ";
                }
                else
                {
                    updateSql += primaryKey + "='" + row[primaryKey] + "' and ";
                }
            }
            return updateSql.Substring(0, updateSql.Length - 4);
        }

    }
}
