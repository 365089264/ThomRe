using System.Data.OleDb;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using VAV.Entities;
using System;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace VAV.DAL
{
    public class WMPBaseRepository
    {
        /// <summary>
        /// Non-query store procedure
        /// </summary>
        /// <param name="inName">sp name</param>
        /// <param name="inParms">parameters</param>
        protected void ExecNonQuerySp(string inName, OracleParameter[] inParms)
        {
            using (var WMPDB = new Genius_HistEntities())
            {
                WMPDB.Database.Connection.Open();
                DbCommand cmd = WMPDB.Database.Connection.CreateCommand();
                cmd.CommandText = inName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddRange(inParms);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Get data by store procedure
        /// </summary>
        /// <param name="inName">sp name</param>
        /// <param name="inParms">parameters</param>
        /// <returns></returns>
        protected DataSet GetDataSetBySp(string inName, OracleParameter[] inParms)
        {
            using (var WMPDB = new Genius_HistEntities())
            {
                using (var cmd = new OracleCommand())
                {
                    DataSet ds = null;

                    WMPDB.Database.Connection.Open();
                    cmd.Connection = (OracleConnection)WMPDB.Database.Connection;
                    cmd.CommandText = inName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;

                    if (inParms != null)
                        cmd.Parameters.AddRange(inParms);

                    var da = new OracleDataAdapter(cmd);
                    ds = new DataSet();
                    da.Fill(ds);
                    return ds;
                }
            }
        }

        /// <summary>
        /// Get data by store procedure
        /// </summary>
        /// <param name="inName">sp name</param>
        /// <param name="inParms">parameters</param>
        /// <returns></returns>
        protected DataSet GetDataSetBySp(string inName, OracleParameter[] inParms, string outName, out object value)
        {
            using (var WMPDB = new Genius_HistEntities())
            {
                using (var cmd = new OracleCommand())
                {
                    DataSet ds = null;

                    WMPDB.Database.Connection.Open();
                    cmd.Connection = (OracleConnection)(WMPDB.Database.Connection);
                    cmd.CommandText = inName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;

                    if (inParms != null)
                        cmd.Parameters.AddRange(inParms);

                    var da = new OracleDataAdapter(cmd);
                    ds = new DataSet();
                    da.Fill(ds);
                    value = cmd.Parameters[outName].Value;
                    
                    return ds;
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
