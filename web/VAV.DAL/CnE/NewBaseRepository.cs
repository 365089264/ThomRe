using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using VAV.Entities;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;
using OracleDataAdapter = Oracle.ManagedDataAccess.Client.OracleDataAdapter;
using OracleParameter = Oracle.ManagedDataAccess.Client.OracleParameter;

namespace VAV.DAL.CnE
{
   public class NewBaseRepository
    {
        /// <summary>
        /// Non-query store procedure
        /// </summary>
        /// <param name="inName">sp name</param>
        /// <param name="inParms">parameters</param>
       protected void ExecNonQuerySp(string inName, OracleParameter[] inParms)
       {
           using (var cnEDB = new CneNewEntities())
           {
               cnEDB.Database.Connection.Open();
               DbCommand cmd = cnEDB.Database.Connection.CreateCommand();
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
           using (var cnEDB = new CneNewEntities())
           {
               using (OracleCommand spCmd = new OracleCommand())
               {
                   DataSet ds = null;

                   cnEDB.Database.Connection.Open();
                   spCmd.Connection = new OracleConnection(cnEDB.Database.Connection.ConnectionString);
                   spCmd.CommandText = inName;
                   spCmd.CommandType = CommandType.StoredProcedure;
                   spCmd.CommandTimeout = 0;

                   if (inParms != null)
                       spCmd.Parameters.AddRange(inParms);

                   OracleDataAdapter da = new OracleDataAdapter(spCmd);
                   ds = new DataSet();
                   da.Fill(ds);

                   return ds;
               }
           }
       }

        public DataTable GetDataPaged(string tableName, string strGetFields, string strOrder, string strWhere, int pageIndex, int pageSize, int doCount, int isExcel, out int recordCount)
        {
            var paramArray = new[]
                            {                                  
                                new OracleParameter("tblName", OracleDbType.NVarchar2) { Value = tableName },
                                new OracleParameter("strGetFields", OracleDbType.NVarchar2) { Value = strGetFields },
                                new OracleParameter("strOrder", OracleDbType.NVarchar2) { Value = strOrder },
                                new OracleParameter("strWhere", OracleDbType.NVarchar2) { Value = strWhere },
                                new OracleParameter("pageIndex", OracleDbType.Int32) { Value = pageIndex },
                                new OracleParameter("pageSize", OracleDbType.Int32) { Value = pageSize },
                                new OracleParameter("recordCount", OracleDbType.Int32,ParameterDirection.Output) { Value = pageSize },
                                new OracleParameter("doCount", OracleDbType.Int32) { Value = doCount },
                                new OracleParameter("isExcelReport", OracleDbType.Int32) { Value = isExcel },
                                new OracleParameter("O_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
                                
                            };

            using (var cnEDB = new CneNewEntities())
            {
                using (var spCmd = new OracleCommand())
                {
                    cnEDB.Database.Connection.Open();
                    spCmd.Connection = (OracleConnection)(cnEDB.Database.Connection);
                    spCmd.CommandText = "GetDataPaged";
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;

                    spCmd.Parameters.AddRange(paramArray);
                    var da = new OracleDataAdapter(spCmd);
                    var ds = new DataSet();
                    da.Fill(ds);
                    recordCount = Convert.ToInt32(spCmd.Parameters["recordCount"].Value.ToString());
                    return ds.Tables[0];
                }
            }

        }

        public void Dispose()
        {
        }
    }
}
