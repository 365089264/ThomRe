using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using VAV.DAL.Services;
using VAV.Entities;
using Microsoft.Practices.Unity;
using VAV.Model.Data;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;
using OracleDataAdapter = Oracle.ManagedDataAccess.Client.OracleDataAdapter;
using OracleParameter = Oracle.ManagedDataAccess.Client.OracleParameter;

namespace VAV.DAL.Report
{
    public class PartnersReportRepository
    {
        [Dependency]
        public IFileService FileService { get; set; }

        public DataTable GetChinaSecurities(string category, string title, string code, DateTime startDate, DateTime endDate, out int total, int startPage = 1, int pageSize = 50)
        {
            var paramArray = new[]
                            {
                                new OracleParameter("P_Category", OracleDbType.Varchar2) { Value = category },
                                new OracleParameter("P_Title", OracleDbType.Varchar2) { Value = title },
                                new OracleParameter("P_Code", OracleDbType.Varchar2) { Value = code },
                                new OracleParameter("P_StartDate", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)startDate }, 
                                new OracleParameter("P_EndDate", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)endDate },
                                new OracleParameter("P_StartPage", OracleDbType.Int32) { Value = startPage },
                                new OracleParameter("P_PageSize", OracleDbType.Int32) { Value = pageSize },
                                new OracleParameter("P_TOTAL", OracleDbType.Int32,ParameterDirection.Output) { Value = 0 },
                                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
                            };
            using (var cmaDb = new CMAEntities())
            {
                using (OracleCommand spCmd = new OracleCommand())
                {
                    cmaDb.Database.Connection.Open();
                    spCmd.Connection = new OracleConnection(cmaDb.Database.Connection.ConnectionString);
                    spCmd.CommandText = "GetChinaSecurities";
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;

                    spCmd.Parameters.AddRange(paramArray);

                    OracleDataAdapter da = new OracleDataAdapter(spCmd);
                    var ds = new DataSet();
                    da.Fill(ds);
                    total = Convert.ToInt32(spCmd.Parameters["P_TOTAL"].Value.ToString());
                    return ds.Tables[0];
                }
            }
        }

        #region Homepage
        public IEnumerable<HOMEMODULE> GetHomeModules()
        {
            using (var cmadb = new CMAEntities())
            {
                return cmadb.HOMEMODULEs.Where(m => m.ISVALID.Value == 1).Include(m => m.HOMEITEMs).ToList();
            }
        }

        public List<HOMEANNOUNCEMENT> GetHomeAnnouncements()
        {
            using (var vavdb = new CMAEntities())
            {
                return vavdb.HOMEANNOUNCEMENTs.Where(re => re.ISVALID == 1).ToList();
            }
        }

        public void UploadFile(int id,string moduleId, string titleCn, string titleEn, string descrCn, string descrEn,
            string uploadType, string uploadTypeValue, string typeParam, bool isvalid, string submitter, DateTime submittDate, string fileType, byte[] doc)
        {
            var paramArray = new[]
            {
                new OracleParameter("P_Id", OracleDbType.Int32) {Value = id},
                new OracleParameter("P_ModuleId", OracleDbType.Int32) {Value = Convert.ToInt32(moduleId)},
                new OracleParameter("P_TitleCn", OracleDbType.NVarchar2) {Value = titleCn},
                new OracleParameter("P_TitleEn", OracleDbType.NVarchar2) {Value = titleEn},
                new OracleParameter("P_SubmitDate", OracleDbType.TimeStamp) {Value = (OracleTimeStamp)submittDate},
                new OracleParameter("P_Submitter", OracleDbType.NVarchar2) {Value = submitter},
                new OracleParameter("P_UploadType", OracleDbType.Varchar2) {Value = uploadType},
                new OracleParameter("P_UploadTypeValue", OracleDbType.NVarchar2) {Value = uploadTypeValue},
                new OracleParameter("P_TypeParam", OracleDbType.NVarchar2) {Value = typeParam},
                new OracleParameter("P_Isvalid", OracleDbType.Int32) {Value = isvalid?1:0},
                new OracleParameter("P_FileType", OracleDbType.NVarchar2) {Value = fileType},
                new OracleParameter("P_OutPara", OracleDbType.Int32,ParameterDirection.Output) { Value = 0 },
                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };

            string fileId = "";
            using (var cmaDb = new CMAEntities())
            {
                cmaDb.Database.Connection.Open();
                DbCommand spCmd = cmaDb.Database.Connection.CreateCommand();
                spCmd.CommandText = "UploadHomeItem";
                spCmd.CommandType = CommandType.StoredProcedure;
                spCmd.CommandTimeout = 0;

                spCmd.Parameters.AddRange(paramArray);

                spCmd.ExecuteNonQuery();
                fileId = spCmd.Parameters["P_OutPara"].Value.ToString();
            }
            if (!string.IsNullOrEmpty(fileId) && !string.IsNullOrEmpty(fileType))
                FileService.UploadFile("/VAV/Home", fileId + "." + fileType, doc);
        }
        public DataTable GetItemFile(string ID)
        {
            var paramArray = new[]
            {
                new OracleParameter("P_ID", OracleDbType.Int32) {Value = Convert.ToInt32(ID)},
                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            using (var cmaDb = new CMAEntities())
            {
                using (OracleCommand spCmd = new OracleCommand())
                {
                    cmaDb.Database.Connection.Open();
                    spCmd.Connection = new OracleConnection(cmaDb.Database.Connection.ConnectionString);
                    spCmd.CommandText = "GetHomeItem";
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;

                    spCmd.Parameters.AddRange(paramArray);

                    OracleDataAdapter da = new OracleDataAdapter(spCmd);
                    var ds = new DataSet();
                    da.Fill(ds);
                    return ds.Tables[0];
                }
            }
        }

        public int DeleteHomeItem(int ID)
        {
            var paramArray = new[]
            {
                new OracleParameter("P_ID",OracleDbType.Int32) { Value = ID }
            };
            using (var cmaDb = new CMAEntities())
            {
                cmaDb.Database.Connection.Open();
                DbCommand spCmd = cmaDb.Database.Connection.CreateCommand();
                spCmd.CommandText = "DeleteHomeItemByID";
                spCmd.CommandType = CommandType.StoredProcedure;
                spCmd.CommandTimeout = 0;

                spCmd.Parameters.AddRange(paramArray);

                return spCmd.ExecuteNonQuery();
            }
        }
        public DataTable GetHomeItemAll()
        {
            var paramArray = new[]
            {
                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            using (var cmaDb = new CMAEntities())
            {
                using (OracleCommand spCmd = new OracleCommand())
                {
                    cmaDb.Database.Connection.Open();
                    spCmd.Connection = new OracleConnection(cmaDb.Database.Connection.ConnectionString);
                    spCmd.CommandText = "GetHomeItemAll";
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;

                    spCmd.Parameters.AddRange(paramArray);

                    OracleDataAdapter da = new OracleDataAdapter(spCmd);
                    var ds = new DataSet();
                    da.Fill(ds);
                    return ds.Tables[0];
                }
            }
        }
        public DataTable GetHomeItems(int moduleId,string itemName,int startPage,int pageSize, out int total)
        {
            var paramArray = new[]
            {
                new OracleParameter("P_ModuleId", OracleDbType.Int32) {Value = moduleId},
                new OracleParameter("P_ItemName", OracleDbType.Varchar2) {Value = itemName},
                new OracleParameter("P_StartPage", OracleDbType.Int32) {Value = startPage},
                new OracleParameter("P_PageSize", OracleDbType.Int32) {Value = pageSize},
                new OracleParameter("P_Total", OracleDbType.Int32, ParameterDirection.Output),
                new OracleParameter("P_CUR", OracleDbType.RefCursor) {Direction = ParameterDirection.Output}
            };
            using (var cmaDb = new CMAEntities())
            {
                using (OracleCommand spCmd = new OracleCommand())
                {
                    cmaDb.Database.Connection.Open();
                    spCmd.Connection = new OracleConnection(cmaDb.Database.Connection.ConnectionString);
                    spCmd.CommandText = "GetHomeItems";
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;

                    spCmd.Parameters.AddRange(paramArray);

                    OracleDataAdapter da = new OracleDataAdapter(spCmd);
                    var ds = new DataSet();
                    da.Fill(ds);
                    object value;
                    value = spCmd.Parameters["P_Total"].Value;
                    total = value.ToString() != "null" ? Convert.ToInt32(value.ToString()) : 0;
                    return ds.Tables[0];
                }
            }
        }


        public CmaFile GetHomeItemFileData(int id)
        {
            return FileService.GetFileById(id, "VAV");
        }

        #endregion
    }
}
