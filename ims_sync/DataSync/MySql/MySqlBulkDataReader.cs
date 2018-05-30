using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace Luna.DataSync.Core
{
    public class MySqlBulkDataReader : BaseBulkDataReader
    {
        #region Constructors
        public MySqlBulkDataReader(
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
            var conn = new MySqlConnection();
            conn.ConnectionString = connString;
            return conn;
        }

        protected override IDbCommand GetDbCommand(string cmdText, IDbConnection conn)
        {
            cmdText = cmdText.Replace("STATUS", "(case STATUS when true then 1 else 0 end) 'STATUS'");
            return new MySqlCommand(cmdText, (MySqlConnection)conn);
        }

        protected override IDbDataAdapter GetDbDataAdapter(IDbCommand cmd)
        {
            return new MySqlDataAdapter((MySqlCommand)cmd);
        }
        #endregion
    }
}
