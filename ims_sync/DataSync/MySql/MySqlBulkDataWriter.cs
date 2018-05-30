using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Text;

namespace Luna.DataSync.Core
{
    public class MySqlBulkDataWriter : BaseBulkDataWriter
    {
        #region Constructors
        public MySqlBulkDataWriter(string connString, int sqlCommandTimeout)
            : base(connString, sqlCommandTimeout)
        {
        }
        #endregion

        #region Protected Methods
        protected override IDbConnection GetDbConnection(string connString)
        {
            return new MySqlConnection(connString);
        }

        protected override IDbCommand GetDbCommand(string cmdText, IDbConnection conn, IDbTransaction trans)
        {
            return new MySqlCommand(cmdText, (MySqlConnection)conn, (MySqlTransaction)trans);
        }

        protected override IsolationLevel GetDbIsolationLevel()
        {
            return IsolationLevel.ReadCommitted;
        }

        protected override string GetTopOneRecordFromTableSQLString(string tableName)
        {
            return string.Format("SELECT * FROM {0} limit 1", tableName);
        }

        protected override string GetDateTimeFormatString()
        {
            return "yyyy-MM-dd HH:mm:ss";//hh -> HH  .by yy
        }

        protected override string GetDateFormatString()
        {
            return "yyyy-MM-dd";//hh -> HH  .by yy
        }

        protected override string GetTimestampFormatString()
        {
            throw new NotImplementedException();
        }

        protected override IDataParameter GetDataParameter(string paraName, DbColumnType columnType, object value)
        {
            return null;
        }

        protected override string GetParaSpliter()
        {
            return "@";
        }

        protected override IDbCommand GetDbCommand(string cmdText, IDbConnection conn)
        {
            return new MySqlCommand(cmdText, (MySqlConnection) conn);
        }

        #endregion
    }
}
