using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Luna.DataSync.Core
{
    public class SqlServerBulkDataWritter : BaseBulkDataWriter
    {
        #region Constructors
        public SqlServerBulkDataWritter(string connString, int sqlCommandTimeout)
            :base(connString, sqlCommandTimeout)
        {
        }
        #endregion

        #region Protected Methods
        protected override IDbConnection GetDbConnection(string connString)
        {
            return new SqlConnection(connString);
        }

        protected override IDbCommand GetDbCommand(string cmdText, IDbConnection conn, IDbTransaction trans)
        {
            return new SqlCommand(cmdText, (SqlConnection)conn, (SqlTransaction)trans);
        }

        protected override IsolationLevel GetDbIsolationLevel()
        {
            return IsolationLevel.Snapshot;
        }

        protected override string GetTopOneRecordFromTableSQLString(string tableName)
        {
            return string.Format("SELECT top 1 * FROM {0}", tableName);
        }

        protected override string GetDateTimeFormatString()
        {
            return "yyyy-MM-dd hh:mm:ss tt";
        }

        protected override string GetDateFormatString()
        {
            return "yyyy-MM-dd";
        }

        protected override string GetTimestampFormatString()
        {
            throw new NotImplementedException();
        }

        protected override IDataParameter GetDataParameter(string paraName, DbColumnType columnType, object value)
        {
            var type = SqlDbType.Int;
            switch (columnType)
            {
                case DbColumnType.INT:
                    type = SqlDbType.Int;
                    break;
                case DbColumnType.DECIMAL:
                    type = SqlDbType.Decimal;
                    break;
                case DbColumnType.DATETIME:
                    type = SqlDbType.DateTime;
                    break;
                case DbColumnType.CHAR:
                    type = SqlDbType.Char;
                    break;
                case DbColumnType.VARCHAR:
                    type = SqlDbType.VarChar;
                    break;
                case DbColumnType.NCHAR:
                    type = SqlDbType.NChar;
                    break;
                case DbColumnType.NVARCHAR:
                    type = SqlDbType.NVarChar;
                    break;
                case DbColumnType.TEXT:
                    type = SqlDbType.Text;
                    break;
                case DbColumnType.NTEXT:
                    type = SqlDbType.NText;
                    break;
                default:
                    break;
            }
            return new SqlParameter(paraName, type) { Value = value };
        }

        protected override string GetParaSpliter()
        {
            return "@";
        }

        protected override IDbCommand GetDbCommand(string cmdText, IDbConnection conn)
        {
            return new SqlCommand(cmdText, (SqlConnection)conn);
        }

        #endregion
    }
}
