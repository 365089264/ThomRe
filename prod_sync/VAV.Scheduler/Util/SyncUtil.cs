using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAVToolsEntities;
using System.Data.Common;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;
using OracleParameter = Oracle.ManagedDataAccess.Client.OracleParameter;


namespace VAV.Scheduler.Util
{
    public static class SyncUtil
    {
        public static void UpdateBondInfo(DateTime lastSyncTime, DateTime currentSyncTime)
        {

            using (var Db = new BondDBEntities())
            {
                try
                {
                    Db.Database.Connection.Open();
                    DbCommand cmd = Db.Database.Connection.CreateCommand();
                    cmd.CommandText = "UpdateBondInfo";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddRange(new[] {
                        new OracleParameter("LastSyncTime", OracleDbType.TimeStamp) { Value = lastSyncTime },
                        new OracleParameter("CurrentSyncTime", OracleDbType.TimeStamp) { Value = currentSyncTime }
                    });

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    Db.Database.Connection.Close();
                }
            }
        }


        public static string UpdateFileTopic(DateTime lastSyncTime, DateTime currentSyncTime)
        {
            var result = "Success";

            using (var IPPDB = new IPPEntities())
            {
                try
                {
                    IPPDB.Database.Connection.Open();
                    var cmd = IPPDB.Database.Connection.CreateCommand(); ;
                    cmd.CommandText = "UpdateFileTopic";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddRange(new[] {
                        new OracleParameter("V_LastSyncTime", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)lastSyncTime },
                        new OracleParameter("V_CurrentSyncTime", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)currentSyncTime }
                    });

                    OracleParameter outPar = new OracleParameter();
                    outPar.ParameterName = "V_Result";
                    outPar.DbType = DbType.String;
                    outPar.Size = 400;
                    outPar.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outPar);

                    cmd.ExecuteNonQuery();

                    var ret = (string)cmd.Parameters["V_Result"].Value;
                    result = ret.Split('!').FirstOrDefault() == "Success" ? "Success" : "Fail";
                }
                catch (Exception ex)
                {
                    result = "Fail" + ex.Message;
                }
                finally
                {
                    IPPDB.Database.Connection.Close();
                }

                return result;
            }
        }

        /*empty table Iss_def */
        public static string Delete_Iss_Def()
        {
            var result = "Success";

            using (var Db = new VAVEntities())
            {
                try
                {
                    Db.Database.Connection.Open();
                    DbCommand cmd = Db.Database.Connection.CreateCommand();
                    cmd.CommandText = "vav.DeleteIssDef";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                                       OracleParameter outPar = new OracleParameter();
                    outPar.ParameterName = "@Result";
                    outPar.DbType = DbType.String;
                    outPar.Size = 400;
                    outPar.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outPar);

                    cmd.ExecuteNonQuery();

                    var ret = (string)cmd.Parameters["@Result"].Value;
                    result = ret.Split('!').FirstOrDefault() == "Success" ? "Success" : "Fail";
                }
                catch (Exception ex)
                {
                    result = "Fail" + ex.Message;
                }
                finally
                {
                    Db.Database.Connection.Close();
                }

                return result;
            }
        }
    }
}
