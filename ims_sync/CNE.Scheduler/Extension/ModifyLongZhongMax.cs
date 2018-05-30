using System;
using System.Configuration;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;

namespace CNE.Scheduler.Extension
{
    public class ModifyLongZhongMax
    {
        private OracleConnection conn = null;
        private OracleTransaction tran = null;
        public void ExecuteMax()
        {
            try
            {
                using (conn = new OracleConnection(ConfigurationManager.AppSettings["mergeData"]))
                {

                    conn.Open();
                    tran = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                    using (OracleCommand cmd = new OracleCommand())
                    {
                        cmd.Connection = conn;
                        cmd.Transaction = tran;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "createMaxChemicalIndustryTable";
                        cmd.ExecuteNonQuery();
                    }
                    using (OracleCommand cmd = new OracleCommand())
                    {
                        cmd.Connection = conn;
                        cmd.Transaction = tran;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "createMaxEnergyGasTable";
                        cmd.ExecuteNonQuery();
                    }
                    using (OracleCommand cmd = new OracleCommand())
                    {
                        cmd.Connection = conn;
                        cmd.Transaction = tran;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "createMaxEnergyTable";
                        cmd.ExecuteNonQuery();
                    }
                    using (OracleCommand cmd = new OracleCommand())
                    {
                        cmd.Connection = conn;
                        cmd.Transaction = tran;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "createMaxEnergyOilTable";
                        cmd.ExecuteNonQuery();
                    }
                    tran.Commit();
                }
               
            }
            catch (Exception e)
            {
                tran.Rollback();

            }
            finally
            {
                conn.Close();
                
            }
        }
    }
}
