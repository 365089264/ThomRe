using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Configuration;
using VAV.DAL.Common;
using VAV.Entities;
using System.Web.Mvc;
using System;
using VAV.Model.Data.RSReport;
using System.Threading;
using VAV.DAL.Services;
using Microsoft.Practices.Unity;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;
using OracleDataAdapter = Oracle.ManagedDataAccess.Client.OracleDataAdapter;
using OracleParameter = Oracle.ManagedDataAccess.Client.OracleParameter;


namespace VAV.DAL.ResearchReport
{
    public class ResearchReportRepository
    {
        private static string FileDBConStr = WebConfigurationManager.AppSettings["FileDBConStr"];
        [Dependency]
        public IFileService FileService { get; set; }

        public List<INSTITUTIONINFO> GetInstitutions()
        {
            using (var db = new ResearchReportEntities())
            {
                return db.INSTITUTIONINFOes.Include("FILEDETAIL").Include("FILEDETAIL.FILETYPEINFO").Where(i => i.ISVALID == 1).ToList();
            }
        }

        public FileData GetFileDataById(int id)
        {
            var file = FileService.GetFileById(id, "RR");
            return file == null ? null : new FileData { FileId = (int)file.Id, Content = file.Content };
        }

        public FILEDETAIL GetFileDetailById(int id)
        {
            using (var db = new ResearchReportEntities())
            {
                return db.FILEDETAILs.FirstOrDefault(f => f.FILEID == id);
            }
        }

        public void AddFileVisitLog(int id, string userId)
        {

            using (var db = new ResearchReportEntities())
            {
                try
                {
                    var fileVisitLog = new FILEVISITLOG
                    {
                        FILEID = id,
                        USERID = userId,
                        VISITDATETIME = DateTime.Now,
                        VISITLOGID = db.FILEVISITLOGs.Max(re => re.VISITLOGID) + 1

                    };
                    db.FILEVISITLOGs.Add(fileVisitLog);
                    db.SaveChanges();
                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }


        }

        public byte[] GetInstLogoBytesByCode(string code)
        {
            using (var db = new ResearchReportEntities())
            {
                INSTITUTIONINFO institutionInfo = db.INSTITUTIONINFOes.FirstOrDefault(i => i.CODE == code);
                if (institutionInfo != null)
                {
                    var cmaFile = FileService.GetFileById((long)institutionInfo.ID_C, "LOGO");
                    if (cmaFile != null)
                        return cmaFile.Content;
                }
                return null;
            }
        }

        public DataTable CheckBankFile(string idlist)
        {
            var paramArray = new[]
            {
                new OracleParameter("ids", OracleDbType.NVarchar2)  { Value = idlist },
                new OracleParameter("cur", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            return GetDataSetBySp("CheckFileExist", paramArray).Tables[0];
        }

        public DataTable CheckCommonFileExsit(string idlist, string table, string column)
        {
            var paramArray = new[]
            {
                new OracleParameter("ids", OracleDbType.Varchar2,4000) { Value = idlist },
                new OracleParameter("table_name", OracleDbType.NVarchar2) { Value = table },
                new OracleParameter("column_name", OracleDbType.NVarchar2) { Value = column },
                new OracleParameter("cur", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            return GetDataSetBySp("CheckCommonFileExist", paramArray).Tables[0];
        }

        #region Report Search

        public List<SelectListItem> GetOrgOptions(string orgCode)
        {
            using (var db = new ResearchReportEntities())
            {
                var orgList = (from p in db.INSTITUTIONINFOes
                               where p.ISVALID == 1
                               select p).Distinct().OrderBy(
                                   x => x.CODE).AsEnumerable().Select(x => new SelectListItem { Text = x.DisplayName, Value = x.CODE }).ToList();

                if (orgCode != "all")
                    orgList.Where(o => o.Value == orgCode).ToList().ForEach(o => o.Selected = true);
                else
                    orgList.ForEach(o => o.Selected = true);

                return orgList;
            }
        }

        //reportType need to be englishname
        public List<SelectListItem> GetReportTypeOptions(string orgCode, string reportType)
        {
            List<SelectListItem> typeList;
            List<string> typeCodeList;

            using (var db = new ResearchReportEntities())
            {
                if (orgCode == "all")
                {
                    typeCodeList = (from f in db.FILEDETAILs
                                    select new { f.INSTITUTIONINFOCODE, f.FILETYPECODE })
                                        .AsEnumerable()
                                        .Select(f => f.FILETYPECODE).Distinct().ToList();
                }
                else
                {
                    typeCodeList = (from f in db.FILEDETAILs
                                    select new { f.INSTITUTIONINFOCODE, f.FILETYPECODE })
                                        .AsEnumerable()
                                        .Where(f => orgCode.Contains(f.INSTITUTIONINFOCODE))
                                        .Select(f => f.FILETYPECODE).Distinct().ToList();
                }

                typeList = (from p in db.FILETYPEINFOes
                            where p.ISVALID == 1
                            select p).Distinct()
                              .OrderBy(x => x.CODE)
                              .AsEnumerable()
                              .Where(f => typeCodeList.Contains(f.CODE))
                              .Select(x => new SelectListItem { Text = x.DisplayName, Value = x.FILENAMEEN }).Distinct(new UsuersComparer()).ToList();

                if (orgCode == "all" || reportType == "all")
                    typeList.ForEach(t => t.Selected = true);
                else
                    typeList.Where(t => t.Value == reportType).ToList().ForEach(t => t.Selected = true);

                return typeList;
            }
        }

        public IEnumerable<ReportDetail> GetRSReport(DateTime startDate, DateTime endDate, string orgCodes, string reportTypes, string reportName, int pageNo, int pageSize, out int total)
        {
            var dbHelper = new OracleDBHelper(FileDBConStr);
            var parameters = new[]
            {
                new OracleParameter("I_REPORTNAME", OracleDbType.NVarchar2){Value = reportName},
                new OracleParameter("I_INSTITUTIONINFOCODE", OracleDbType.Varchar2){Value = orgCodes},
                new OracleParameter("I_REPORTTYPE", OracleDbType.NVarchar2){Value = reportTypes},
                new OracleParameter("I_STARTDATE", OracleDbType.TimeStamp){Value = (OracleTimeStamp)startDate},
                new OracleParameter("I_ENDDATE", OracleDbType.TimeStamp){Value = (OracleTimeStamp)endDate},
                new OracleParameter("I_CULTRURENAME", OracleDbType.NVarchar2){Value = Thread.CurrentThread.CurrentUICulture.Name},
                new OracleParameter("I_PAGENO", OracleDbType.Int32){Value = pageNo},
                new OracleParameter("I_PAGESIZE", OracleDbType.Int32){Value = pageSize},
                new OracleParameter("O_TOTAL", OracleDbType.Int32){Direction = ParameterDirection.Output},
                new OracleParameter("O_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };

            object outValue;
            var dt = dbHelper.GetDataSetBySp("FILEDETAIL_REPORTSEARCH", parameters, "O_TOTAL", out outValue).Tables[0];
            total = string.IsNullOrEmpty(outValue.ToString()) ? 0 : Convert.ToInt32(outValue.ToString());
            var result = (from DataRow row in dt.Rows
                          select new ReportDetail
                          {
                              FileId = Convert.ToInt32(row["FILEID"]),
                              DisplayName = Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? row["FILENAMECN"].ToString() : row["FILENAMEEN"].ToString(),
                              FileTypeCode = row["FILENAMEEN"].ToString(), //english name as code
                              FileType = Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? row["FILETYPECN"].ToString() : row["FILETYPEEN"].ToString(),
                              InstitutionName = Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? row["INSTITUTIONNAMECN"].ToString() : row["INSTITUTIONNAMEEN"].ToString(),
                              InstitutionInfoCode = row["INSTITUTIONINFOCODE"].ToString(),
                              ReportDate = Convert.ToDateTime(row["REPORTDATE"]),
                              DisplayDate = ((DateTime)row["REPORTDATE"]).ToString("yyyy-MM-dd"),
                              Author = row["AUTHOR"].ToString(),
                              Ext = row["EXTENSION"].ToString()
                          }).ToList();
            return result;
        }

        #endregion


        /// <summary>
        /// Get data by store procedure
        /// </summary>
        /// <param name="inName">sp name</param>
        /// <param name="inParms">parameters</param>
        /// <returns></returns>
        protected DataSet GetDataSetBySp(string inName, OracleParameter[] inParms)
        {
            using (var cmafdb = new ResearchReportEntities())
            {
                using (OracleCommand spCmd = new OracleCommand())
                {
                    DataSet ds = null;

                    cmafdb.Database.Connection.Open();
                    spCmd.Connection = new OracleConnection(cmafdb.Database.Connection.ConnectionString);
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


    }

    public class UsuersComparer : IEqualityComparer<SelectListItem>
    {
        public bool Equals(SelectListItem x, SelectListItem y)
        {
            if (x.Text == y.Text && x.Value == y.Value)
                return true;

            return false;
        }

        public int GetHashCode(SelectListItem obj)
        {
            int hasText = obj.Text == null ? 0 : obj.Text.GetHashCode();
            int hasValue = obj.Value == null ? 0 : obj.Value.GetHashCode();

            return hasText ^ hasValue;
        }
    }
}
