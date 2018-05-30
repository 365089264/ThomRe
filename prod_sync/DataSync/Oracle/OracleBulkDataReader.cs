using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;

namespace Luna.DataSync.Core
{
    public class OracleBulkDataReader : BaseBulkDataReader
    {
        #region Constructors
        public OracleBulkDataReader(
            string connString,
            int sqlCommandTimeout,
            DateTime lastSyncTime,
            DateTime currentSyncTime,
            List<string> customBonds)
            : base(connString, sqlCommandTimeout, lastSyncTime, currentSyncTime, customBonds)
        {
        }
        #endregion

        #region Protected Members
        protected override IDbConnection GetDbConnection(string connString)
        {
            return new OracleConnection(connString);
        }

        protected override IDbCommand GetDbCommand(string cmdText, IDbConnection conn)
        {
            return new OracleCommand(cmdText, (OracleConnection)conn);
        }

        protected override IDbDataAdapter GetDbDataAdapter(IDbCommand cmd)
        {
            return new OracleDataAdapter((OracleCommand)cmd);
        }
        #endregion
    }
}
