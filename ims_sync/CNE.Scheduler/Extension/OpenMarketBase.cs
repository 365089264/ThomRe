using System;
using System.Text;
using CNE.Scheduler.Extension.Model;

namespace CNE.Scheduler.Extension
{
    public class OpenMarketBase
    {
        public virtual void Init(OpenMarketOperation data, QueueMessageFromRFA message)
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

            bool isExist = OracleHelper.Exists("select count(*) from " + data._tabName + " where ric='" + data._ric + "' and issuedate='" + data._values[idx] + "'");
            if (isExist)
            {
                UpdateSql(data);
                OracleHelper.ExecuteSql(data._bulkSqlStatments.ToString());
                message.OperationType = "Update";
            }
            else
            {
                InsertSql(data);
                OracleHelper.ExecuteSql(data._bulkSqlStatments.ToString());
                message.OperationType = "Insert";
            }
            message.ExecSql = data._bulkSqlStatments.ToString();
            message.ReturnMessage = " <span style=\"color:green;\">Success!</span>";

        }
        protected virtual void InsertSql(OpenMarketOperation data)
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
                sqlValues.Append(seprator + quot + data._values[i].ToString().Trim() + quot);

            }

            sqlColNames.Append(seprator + "CREATEDATE");
            sqlValues.Append(seprator + "to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-mm-dd hh24:mi:ss')");
            sqlColNames.Append(seprator + "MODIFYDATE");
            sqlValues.Append(seprator + "to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-mm-dd hh24:mi:ss')");

            string sql = "INSERT INTO " + data._tabName + "(RIC," + sqlColNames + ")"
                         + " VALUES('" + data._ric + "', " + sqlValues + ")";
            data._bulkSqlStatments.Append(sql);
            data._values.Clear();
            data.setRic(null);
        }
        protected virtual void UpdateSql(OpenMarketOperation data)
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
                sqlCol.Append(data._values[i].ToString().Trim() + quot);

            }

            sqlCol.Append(seprator + "MODIFYDATE=");
            sqlCol.Append("to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-mm-dd hh24:mi:ss')");

            int idx = data.getColIdx("issueDate");
            string sql = "update " + data._tabName + " set " + sqlCol.ToString()
                         + " where RIC='" + data._ric + "' and  issuedate='" + data._values[idx] + "'";
            data._bulkSqlStatments.Append(sql);
        }

        protected virtual bool IgnoreSql(OpenMarketOperation data,ref string ignoreMessage)
        {
            return false;
        }

    }
}
