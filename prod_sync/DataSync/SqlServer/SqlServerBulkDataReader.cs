using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Luna.DataSync.Core
{
    public class SqlServerBulkDataReader: BaseBulkDataReader
    {
         #region Constructors
        public SqlServerBulkDataReader(
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
            return new SqlConnection(connString);
        }

        protected override IDbCommand GetDbCommand(string cmdText, IDbConnection conn)
        {
            return new SqlCommand(cmdText, (SqlConnection)conn);
        }

        protected override IDbDataAdapter GetDbDataAdapter(IDbCommand cmd)
        {
            return new SqlDataAdapter((SqlCommand)cmd);
        }
        #endregion
    }
}
