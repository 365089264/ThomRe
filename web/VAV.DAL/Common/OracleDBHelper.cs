using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace VAV.DAL.Common
{
    public class OracleDBHelper
    {
        private readonly string connStr;
        public OracleDBHelper(string connStr)
        {
            this.connStr = connStr;
        }
        public int ExecuteStorageWithRevalue(string storName, int returnIndex, params OracleParameter[] paras)
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = storName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddRange(paras);
                    cmd.ExecuteScalar();
                }
            }
            return Convert.ToInt32(paras[returnIndex].Value);
        }
        public void ExecuteStorageWithoutRevalue(string storName, OracleParameter[] paras)
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = storName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddRange(paras);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public DataSet GetDataSetBySp(string inName, OracleParameter[] inParms, string outName, out object outValue)
        {
            using (var conn = new OracleConnection(connStr))
            {
                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = inName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;

                    if (inParms != null)
                        cmd.Parameters.AddRange(inParms);

                    var da = new OracleDataAdapter(cmd);
                    var ds = new DataSet();
                    da.Fill(ds);
                    outValue = cmd.Parameters[outName].Value;
                    return ds;
                }
            }
        }

        public DataTable GetDataTableBySql(string sql, params OracleParameter[] paras)
        {
            DataTable tb = new DataTable();
            using (var conn = new OracleConnection(connStr))
            {
                using (var cmd = new OracleCommand(sql, conn))
                {
                    cmd.Parameters.AddRange(paras);
                    var sda = new OracleDataAdapter(cmd);
                    sda.Fill(tb);
                }
            }
            return tb;
        }
        public object ExecuteScaler(string sql, params OracleParameter[] paras)
        {
            object o = null;
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();

                using (var cmd = new OracleCommand(sql, conn))
                {
                    cmd.Parameters.AddRange(paras);
                    o = cmd.ExecuteScalar();
                }
            }
            return o;
        }
    }
}