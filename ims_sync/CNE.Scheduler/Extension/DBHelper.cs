using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace CNE.Scheduler.Extension
{
    public static class DBHelper
    {
        private static string connStr = ConfigurationManager.AppSettings["reportConnstr"].ToString();
        private static string connLogStr = ConfigurationManager.AppSettings["schedulerLogConnstr"].ToString();
        public static int ExecuteStorageWithRevalue(string storName, int returnIndex, params OracleParameter[] paras)
        {
            using (var conn = new OracleConnection(connStr))
            {
                try
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
                catch
                {
                    return 0;
                }
                finally
                {
                    conn.Close();
                }
            }
            return Convert.ToInt32(paras[returnIndex].Value);
        }
        public static void ExecuteStorageWithoutRevalue(string storName, OracleParameter[] paras)
        {
            using (var conn = new OracleConnection(connStr))
            {
                try
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
                finally
                {
                    conn.Close();
                }
            }
        }

        public static void ExecuteLogStorage(string storName, OracleParameter[] paras)
        {
            using (var conn = new OracleConnection(connLogStr))
            {
                var cmd = new OracleCommand();
                try
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = storName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddRange(paras);
                    cmd.Transaction = conn.BeginTransaction();
                    cmd.ExecuteNonQuery();
                    cmd.Transaction.Commit();
                }
                catch (OracleException e)
                {
                    cmd.Transaction.Rollback();
                }
                finally
                {
                    conn.Close();
                }
            }
        }


        public static DataTable GetDataTableBySql(string sql, params OracleParameter[] paras)
        {
            DataTable tb = new DataTable();
            using (var conn = new OracleConnection(connStr))
            {
                try
                {
                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.AddRange(paras);
                        var sda = new OracleDataAdapter(cmd );
                        sda.Fill(tb );
                       
                    }
                }
                finally {
                    conn.Close();
                }
            }
            return tb;
        }
        public static object ExecuteScaler(string sql, params OracleParameter[] paras)
        {
            object o = null;
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                try
                {
                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.AddRange(paras);
                        o = cmd.ExecuteScalar();
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
            return o;
        }
        public static Boolean ExecuteSqlWithTransaction(string sql1, string sql2, byte[] fileData, params OracleParameter[] paras) 
        {
            var isComp = true;
            int primarykey = -1;
            using (var conn = new OracleConnection(connStr))        
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {

                    using (var cmd = new OracleCommand(sql1, conn))
                    {
                        cmd.Transaction = tran;
                        cmd.CommandTimeout = 8000;
                        cmd.Parameters.AddRange(paras);
                        object o = cmd.ExecuteScalar();
                        primarykey = Convert.ToInt32(o);
                    }

                    using (var cmd = new OracleCommand(sql2, conn))
                    {
                        cmd.CommandTimeout = 8000;
                        cmd.Transaction = tran;
                        OracleParameter[] para = {
                                               new OracleParameter ("@id",primarykey),
                                               new OracleParameter ("@content",fileData)
                                           };
                        cmd.Parameters.AddRange(para);
                        cmd.ExecuteNonQuery();
                    }
                    tran.Commit();
                }
                catch { 
                    tran.Rollback();
                    isComp = false;
                }
                finally
                {
                    conn.Close();
                }
            }
            return isComp;
        }
        public static bool ExecuteNoneQuery(string sql, params OracleParameter[] paras)
        {
            var result = true;
            using (var conn = new OracleConnection(connStr))
            {
                try
                {
                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        conn.Open();
                        cmd.Parameters.AddRange(paras);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch
                {
                    result = false;
                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }
    }
}
