using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using VAVToolsEntities;

namespace VAV.Scheduler.Util
{
    public class DBHelper
    {
        public static void ExecNonQuerySp(string inName, OracleParameter[] inParms)
        {
            using (var vav = new VAVEntities())
            {
                vav.Database.Connection.Open();
                DbCommand cmd = vav.Database.Connection.CreateCommand();
                cmd.CommandText = inName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddRange(inParms);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
