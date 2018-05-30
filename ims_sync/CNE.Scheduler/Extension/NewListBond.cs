using System;
using System.Collections.Generic;
using System.Text;
using CNE.Scheduler.Extension.Model;
using System.Data.Odbc;
using System.Data;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace CNE.Scheduler.Extension
{
    public class NewListBond:OpenMarketBase
    {
        public override void Init(OpenMarketOperation data, QueueMessageFromRFA message)
        {

            string ignoreMessage = "";
            bool isIgnore = IgnoreSql(data, ref ignoreMessage);
            if (isIgnore)
            {
                message.OperationType = "Ignore";
                message.ReturnMessage = ignoreMessage;
                return;
            }

            int idx = data.getColIdx("issueDate");

            var obj = GetSingleFromBondDb("select count(*) from " + data._tabName + " where ric='" + data._ric + "' and issuedate='" + data._values[idx] + "'");
            bool isExist = Convert.ToInt32(obj)>0;
            if (isExist)
            {
                UpdateSql(data);
                ExecuteSqlFromBondDb(data._bulkSqlStatments.ToString());
                message.OperationType = "Update";
            }
            else
            {
                InsertSql(data);
                ExecuteSqlFromBondDb(data._bulkSqlStatments.ToString());
                message.OperationType = "Insert";
            }
            message.ExecSql = data._bulkSqlStatments.ToString();
            message.ReturnMessage = " <span style=\"color:green;\">Success!</span>";

        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        protected object GetSingleFromBondDb(string SQLString)
        {
            using (OracleConnection connection = new OracleConnection(ConfigurationManager.AppSettings["BondDBCon"]))
            {
                using (OracleCommand cmd = new OracleCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (OracleException e)
                    {
                        connection.Close();
                        throw new Exception(e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        protected  int ExecuteSqlFromBondDb(string SQLString)
        {
            using (OracleConnection connection = new OracleConnection(ConfigurationManager.AppSettings["BondDBCon"]))
            {
                using (OracleCommand cmd = new OracleCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (OracleException E)
                    {
                        connection.Close();
                        throw new Exception("SQL:" + SQLString + " ;Exception:" + E.Message);
                    }
                }
            }
        }
        protected override void InsertSql(OpenMarketOperation data)
        {
            string seprator = "";
            string quot = "";
            StringBuilder sqlColNames = new StringBuilder();
            StringBuilder sqlValues = new StringBuilder();
            for (int i = 0; i < data._dataTypes.Count; i++)
            {
                if (sqlColNames.Length > 0)
                {
                    seprator = ",";
                }
                if (data._dataTypes[i] == "FLOAT" || data._dataTypes[i] == "INT" || data._values[i] == "null")
                {
                    quot = "";
                }
                else
                {
                    quot = "'";
                }
                sqlColNames.Append(seprator + data._colNames[i]);
                if (data._colNames[i] == "issueAmount" || data._colNames[i] == "accumulatedVolumn")
                {
                    sqlValues.Append(seprator + quot + "10000*" + data._values[i].ToString().Trim() + quot);
                }
                else
                {
                    sqlValues.Append(seprator + quot + data._values[i].ToString().Trim() + quot);
                }

            }

            sqlColNames.Append(seprator + "CREDATE");
            sqlValues.Append(seprator + "to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-mm-dd hh24:mi:ss')");
            string sql = "INSERT INTO " + data._tabName + "(RIC," + sqlColNames + ")"
                         + " VALUES('" + data._ric + "', " + sqlValues + ")";
            data._bulkSqlStatments.Append(sql);
            data._values.Clear();
            data.setRic(null);
        }
        protected override void UpdateSql(OpenMarketOperation data)
        {
            String seprator = "";
            string quot = "";
            StringBuilder sqlCol = new StringBuilder();
            for (int i = 0; i < data._dataTypes.Count; i++)
            {
                if (sqlCol.Length > 0)
                {
                    seprator = ",";
                }
                if (data._dataTypes[i] == "FLOAT" || data._dataTypes[i] == "INT" || data._values[i] == "null")
                {
                    quot = "";
                }
                else
                {
                    quot = "'";
                }

                sqlCol.Append(seprator + data._colNames[i] + "=" + quot);

                if (data._colNames[i] == "issueAmount" || data._colNames[i] == "accumulatedVolumn")
                {
                    sqlCol.Append("10000*" + data._values[i].ToString().Trim() + quot);
                }
                else
                {
                    sqlCol.Append(data._values[i].ToString().Trim() + quot);
                }

            }

            sqlCol.Append(seprator + "CREDATE=");
            sqlCol.Append("to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-mm-dd hh24:mi:ss')");

            int idx = data.getColIdx("issueDate");
            string sql = "update " + data._tabName + " set " + sqlCol.ToString()
                         + " where RIC='" + data._ric + "' and  issuedate='" + data._values[idx] + "'";
            data._bulkSqlStatments.Append(sql);
        }

        public static void GetNewBondRics(List<RmdsRic> rics)
        {
            string connString = ConfigurationManager.AppSettings["EJVCon"];
            OdbcConnection con = new OdbcConnection(connString);
            string sql = "select DISTINCT 'CN'+Z.id_number+'=CFXS' from govcorp..asset X inner join govcorp..asset_exchanges Y on X.asset_id = Y.asset_id inner join govcorp..asset_ident Z on X.asset_id = Z.asset_id where exch_cd = 'CFS' and Z.id_cd = 'CHN' and Y.listing_dt = '" + DateTime.Now.ToString("yyyy-MM-dd") + "'";
            OdbcCommand cmd = new OdbcCommand(sql, con);
            DataSet ds = new DataSet();
            OdbcDataAdapter da = new OdbcDataAdapter(cmd);
            da.Fill(ds);

            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                rics.Add(new RmdsRic { Ric = ds.Tables[0].Rows[i][0].ToString(), Rictype = "CFXS/NEWISSUE" });
            }
        }
    }
}
