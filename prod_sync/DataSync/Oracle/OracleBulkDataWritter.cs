using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;

namespace Luna.DataSync.Core
{
    class OracleBulkDataWritter: BaseBulkDataWriter
    {
        #region Constructors
        public OracleBulkDataWritter(string connString, int sqlCommandTimeout)
            : base(connString, sqlCommandTimeout)
        {
        }
        #endregion

        #region Protected Methods
        protected override IDbConnection GetDbConnection(string connString)
        {
            return new OracleConnection(connString);
        }

        protected override IDbCommand GetDbCommand(string cmdText, IDbConnection conn, IDbTransaction trans)
        {
            return new OracleCommand(cmdText, (OracleConnection)conn, (OracleTransaction)trans);
        }

        protected override IDbCommand GetDbCommand(string cmdText, IDbConnection conn)
        {
            return new OracleCommand(cmdText, (OracleConnection)conn);
        }

        protected override IsolationLevel GetDbIsolationLevel()
        {
            return IsolationLevel.ReadCommitted;
        }

        protected override string GetTopOneRecordFromTableSQLString(string tableName)
        {
            return string.Format("SELECT * FROM {0} where ROWNUM=1", tableName);
        }

        protected override string GetDateTimeFormatString()
        {
            return "dd-MMM-yyyy hh:mm:ss tt";//hh -> HH  .by yy
        }
        protected override string GetDateFormatString()
        {
            return "dd-MMM-yyyy";//hh -> HH  .by yy
        }

        protected override string GetTimestampFormatString()
        {
            return "dd-MMM-yyyy hh:mm:ss.ffffff tt";
        }

        protected override IDataParameter GetDataParameter(string paraName, DbColumnType columnType, object value)
        {
            var type = OracleType.Number;

            switch (columnType)
            {
                case DbColumnType.INT:
                case DbColumnType.DECIMAL:
                    type = OracleType.Number;
                    break;
                case DbColumnType.DATETIME:
                case DbColumnType.DATE:
                    type = OracleType.DateTime;
                    break;
                case DbColumnType.CHAR:
                    type = OracleType.Char;
                    break;
                case DbColumnType.VARCHAR:
                case DbColumnType.VARCHAR2:
                    type = OracleType.VarChar;
                    break;
                case DbColumnType.NCHAR:
                    type = OracleType.NChar;
                    break;
                case DbColumnType.NVARCHAR:
                    type = OracleType.NVarChar;
                    break;
                case DbColumnType.NTEXT:
                    type = OracleType.NClob;
                    break;
                case DbColumnType.CLOB:
                    type = OracleType.Clob;
                    break;
                case DbColumnType.TIMESTAMP:
                    type = OracleType.Timestamp;
                    break;
                default:
                    break;
            }

            return new OracleParameter(paraName, type) { Value = BuildValueForParameters(value, columnType) };
        }

        protected override string GetParaSpliter()
        {
            return ":";
        }
        #endregion
    }
}