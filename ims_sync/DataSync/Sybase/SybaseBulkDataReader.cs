using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Odbc;

namespace Luna.DataSync.Core
{
    public class SybaseBulkDataReader: BaseBulkDataReader
    {

        #region Constructors
        public SybaseBulkDataReader(
            string connString,
            int sqlCommandTimeout,
            DateTime lastSyncTime,
            DateTime currentSyncTime,
            List<string> customBonds)
            :base(connString, sqlCommandTimeout, lastSyncTime, currentSyncTime, customBonds)
        {
        }
        #endregion

        #region Protected Members
        protected override IDbConnection GetDbConnection(string connString)
        {
            return new OdbcConnection(connString);
        }

        protected override IDbCommand GetDbCommand(string cmdText, IDbConnection conn)
        {
            return new OdbcCommand(cmdText, (OdbcConnection)conn);
        }

        protected override IDbDataAdapter GetDbDataAdapter(IDbCommand cmd)
        {
            return new OdbcDataAdapter((OdbcCommand)cmd);
        }
        #endregion
    }
}
