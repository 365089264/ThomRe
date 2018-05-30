using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using VAV.Entities;
using VAV.Model.IPP;
using System.Data.Common;
using VAV.Model.Data.IPP;
using Microsoft.Practices.Unity;
using VAV.DAL.Services;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;
using OracleDataAdapter = Oracle.ManagedDataAccess.Client.OracleDataAdapter;
using OracleParameter = Oracle.ManagedDataAccess.Client.OracleParameter;

namespace VAV.DAL.IPP
{
    public class IPPRepository
    {
        [Dependency]
        public IFileService FileService { get; set; }

        public object QueryTopic(string keyword, bool isEnglish)
        {
            var table = GetDataSetBySp("SearchTopic", new[]
            {
                new OracleParameter("V_key", OracleDbType.NVarchar2,ParameterDirection.InputOutput) { Value = keyword },
                new OracleParameter("cur", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            }).Tables[0];
            return (from DataRow row in table.Rows
                    select new
                               {
                                   ID = row["ID"].ToString(),
                                   Name = isEnglish ? row["NAMEEN"].ToString() : row["NAMECN"].ToString()
                               });
        }

        public List<HomeHotItem> GetTopTopic(string period, bool isEnglish)
        {
            var startDate = DateTime.MinValue;
            switch (period)
            {
                case "1w":
                    startDate = DateTime.Now.AddDays(-7);
                    break;
                case "2w":
                    startDate = DateTime.Now.AddDays(-14);
                    break;
                case "1m":
                    startDate = DateTime.Now.AddMonths(-1);
                    break;
                case "3m":
                    startDate = DateTime.Now.AddMonths(-3);
                    break;
            }
            var paramArray = new[]
            {
                new OracleParameter("startDateTime", OracleDbType.TimeStamp){ Value = (OracleTimeStamp)startDate },
                new OracleParameter("CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            var table = GetDataSetBySp("GETHOTTOPIC", paramArray).Tables[0];
            var data = new List<HomeHotItem>();
            foreach (DataRow row in table.Rows)
            {
                data.Add(new HomeHotItem
                {
                    Hits = row["HITS"] is DBNull ? "0" : row["HITS"].ToString(),
                    ID = row["ID"].ToString(),
                    Name = isEnglish ? row["NAMEEN"].ToString() : row["NAMECN"].ToString()
                });
            }
            return data;
        }

        public DataRow GetTopicByID(int id, string userId)
        {
            var paramArray = new[]
            {
                new OracleParameter("V_TopicID", OracleDbType.Int32){ Value = id },
                new OracleParameter("V_UserID", OracleDbType.NVarchar2){ Value = userId },
                new OracleParameter("CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            return GetDataSetBySp("HITTOPIC", paramArray).Tables[0].Rows[0];
        }



        public int GetTopicIdByFileId(long? fileId)
        {
            using (var IPPDB = new IPPEntities())
            {
                var topicId = (from f in IPPDB.FILEINFOs
                               where f.ID == fileId
                               select f.TOPICID).ToList().FirstOrDefault();

                return (int)topicId;
            }
        }

        public void FollowFile(string userId, long fileId)
        {
            var paramArray = new[]
            {
                new OracleParameter("V_FileID", OracleDbType.Int64) {Value = fileId},
                new OracleParameter("V_UserID", OracleDbType.NVarchar2) {Value = userId}
            };
            GetDataSetBySp("FOLLOWFILE", paramArray);
        }

        public void FollowTopic(string userId, int topicId)
        {
            var paramArray = new[]
            {
                new OracleParameter("V_TopicID", OracleDbType.Int32) {Value = topicId},
                new OracleParameter("V_UserID", OracleDbType.NVarchar2) {Value = userId}
            };
            GetDataSetBySp("FOLLOWTOPIC", paramArray);
        }


        public DataTable QueryFiles(int id, string q, string title, string author, DateTime? startDate, DateTime? endDate, bool enableDate, string description, string source, int pageNo, int pageSize, string order, bool isEn, string userid, out int total, long fileID = 0)
        {

            var paramArray = new[]
                    {
                        new OracleParameter("V_TopicID",OracleDbType.Int32) { Value = id },
                        new OracleParameter("V_q",OracleDbType.NVarchar2,ParameterDirection.InputOutput) { Value = q },
                        new OracleParameter("V_Title",OracleDbType.NVarchar2,ParameterDirection.InputOutput) { Value = title },
                        new OracleParameter("V_Author",OracleDbType.NVarchar2,ParameterDirection.InputOutput) { Value = author },
                        new OracleParameter("V_StartDate",OracleDbType.TimeStamp) { Value = (OracleTimeStamp)startDate },
                        new OracleParameter("V_EndDate",OracleDbType.TimeStamp) { Value = (OracleTimeStamp)endDate },
                        new OracleParameter("V_EnableDate",OracleDbType.Int16) { Value = enableDate?1:0 },
                        new OracleParameter("V_Description",OracleDbType.NVarchar2,ParameterDirection.InputOutput) { Value = description },
                        new OracleParameter("V_Source",OracleDbType.NVarchar2,ParameterDirection.InputOutput) { Value = source },
                        new OracleParameter("V_StartPage",OracleDbType.Int32) { Value = pageNo },
                        new OracleParameter("V_PageSize",OracleDbType.Int32) { Value = pageSize },
                        new OracleParameter("V_Order",OracleDbType.NVarchar2) { Value = order },
                        new OracleParameter("V_IsEn",OracleDbType.Int16) { Value = isEn?1:0 },
                        new OracleParameter("V_UserID",OracleDbType.NVarchar2){ Value = userid},
                        new OracleParameter("V_FileID",OracleDbType.Int64){ Value = fileID},
                        new OracleParameter("O_TOTAL", OracleDbType.Int32){Direction = ParameterDirection.Output},
                        new OracleParameter("O_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
                    };
            object outValue;
            var dt = GetDataSetBySp("QueryFiles", paramArray, "O_TOTAL", out outValue).Tables[0];
            total = string.IsNullOrEmpty(outValue.ToString()) ? 0 : Convert.ToInt32(outValue.ToString());
            return dt;

        }

        public DataTable GetFavoriteFiles(int moduleID, int topicID, string userID, string title, string order,
            int pageNo, int pageSize, bool isEn, out int total)
        {
            var paramArray = new[]
            {
                new OracleParameter("V_ModuleID", OracleDbType.Int32) {Value = moduleID},
                new OracleParameter("V_TopicID", OracleDbType.Int32) {Value = topicID},
                new OracleParameter("V_UserID", OracleDbType.NVarchar2) {Value = userID},
                new OracleParameter("V_Title", OracleDbType.NVarchar2,ParameterDirection.InputOutput) {Value = title},
                new OracleParameter("V_Order", OracleDbType.NVarchar2) {Value = order},
                new OracleParameter("V_StartPage", OracleDbType.Int32) {Value = pageNo},
                new OracleParameter("V_PageSize", OracleDbType.Int32) {Value = pageSize},
                new OracleParameter("V_IsEn", OracleDbType.Int16) {Value = isEn?1:0},
                new OracleParameter("O_TOTAL", OracleDbType.Int32){Direction = ParameterDirection.Output},
               new OracleParameter("O_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            object outValue;
            var dt = GetDataSetBySp("GetFavoriteFiles", paramArray, "O_TOTAL", out outValue).Tables[0];
            total = string.IsNullOrEmpty(outValue.ToString()) ? 0 : Convert.ToInt32(outValue.ToString());
            return dt;
        }


        public DataTable GetFavoriteTopics(int moduleID, string userID, string topic, string order, int pageNo, int pageSize, bool isEn, out int total)
        {
            var paramArray = new[]
            {
                new OracleParameter("V_ModuleID", OracleDbType.Int32) {Value = moduleID},
                new OracleParameter("V_UserID", OracleDbType.NVarchar2) {Value = userID},
                new OracleParameter("V_Topic", OracleDbType.NVarchar2) {Value = topic},
                new OracleParameter("V_Order", OracleDbType.NVarchar2) {Value = order},
                new OracleParameter("V_StartPage", OracleDbType.Int32) {Value = pageNo},
                new OracleParameter("V_PageSize", OracleDbType.Int32) {Value = pageSize},
                new OracleParameter("V_IsEn", OracleDbType.Int16) {Value = isEn?1:0},
                new OracleParameter("O_TOTAL", OracleDbType.Int32){Direction = ParameterDirection.Output},
                new OracleParameter("O_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            object outValue;
            var dt = GetDataSetBySp("GetFavoriteTopics", paramArray, "O_TOTAL", out outValue).Tables[0];
            total = string.IsNullOrEmpty(outValue.ToString()) ? 0 : Convert.ToInt32(outValue.ToString());
            return dt;
        }

        public DataTable GetFilesByStatus(int moduleID, int topicID, string userID, string key, int pageNo, int pageSize, bool isEn, int status,out int total)
        {
            var paramArray = new[]
            {
                new OracleParameter("V_ModuleID",OracleDbType.Int32) { Value = moduleID },
                new OracleParameter("V_TopicID",OracleDbType.Int32) { Value = topicID },
                new OracleParameter("V_UserID",OracleDbType.NVarchar2){ Value = userID}, 
                new OracleParameter("V_Searchkey",OracleDbType.NVarchar2) { Value = key },
                new OracleParameter("V_StartPage",OracleDbType.Int32) { Value = pageNo },
                new OracleParameter("V_PageSize",OracleDbType.Int32) { Value = pageSize },
                new OracleParameter("V_IsEn",OracleDbType.Int16) { Value = isEn?1:0 },
                new OracleParameter("V_Status",OracleDbType.NVarchar2){ Value = status}, 
                new OracleParameter("O_TOTAL", OracleDbType.Int32){Direction = ParameterDirection.Output},
                new OracleParameter("O_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            object outValue;
            var dt = GetDataSetBySp("GetFilesByStatus", paramArray, "O_TOTAL", out outValue).Tables[0];
            total = string.IsNullOrEmpty(outValue.ToString()) ? 0 : Convert.ToInt32(outValue.ToString());
            return dt;
        }

        public DataTable GetFilesForApproval(int moduleID, int topicID, string userID, string key, int pageNo, int pageSize, bool isEn, int status,out int total)
        {
            var paramArray = new[]
            {
                new OracleParameter("V_ModuleID",OracleDbType.Int32) { Value = moduleID },
                new OracleParameter("V_TopicID",OracleDbType.Int32) { Value = topicID },
                new OracleParameter("V_UserID",OracleDbType.NVarchar2){ Value = userID}, 
                new OracleParameter("V_Searchkey",OracleDbType.NVarchar2) { Value = key },
                new OracleParameter("V_Status",OracleDbType.NVarchar2){ Value = status}, 
                new OracleParameter("V_StartPage",OracleDbType.Int32) { Value = pageNo },
                new OracleParameter("V_PageSize",OracleDbType.Int32) { Value = pageSize },
                new OracleParameter("V_IsEn",OracleDbType.Int16) { Value = isEn?1:0 },
                new OracleParameter("O_TOTAL", OracleDbType.Int32){Direction = ParameterDirection.Output},
                new OracleParameter("O_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            object outValue;
            var dt = GetDataSetBySp("GetFilesForApproval", paramArray, "O_TOTAL", out outValue).Tables[0];
            total = string.IsNullOrEmpty(outValue.ToString()) ? 0 : Convert.ToInt32(outValue.ToString());
            return dt;
        }

        public int UpdateFileStatus(int id, int status, string userId)
        {
            var paramArray = new List<OracleParameter>
            {
                new OracleParameter("V_ID",OracleDbType.Int32) { Value = id },
                new OracleParameter("V_Status",OracleDbType.Int32,ParameterDirection.InputOutput) { Value = status },
                new OracleParameter("V_UserId",OracleDbType.NVarchar2) { Value = userId },
            };

            OracleParameter outPar = new OracleParameter();
            outPar.ParameterName = "V_Result";
            outPar.DbType = DbType.Int32;
            outPar.Size = 400;
            outPar.Direction = ParameterDirection.Output;
            paramArray.Add(outPar);

            return ExecNonQuerySpWithResult("UpdateFileStatus", paramArray.ToArray());
        }

        public int DeleteFile(int fileID, string userID)
        {
            var paramArray = new List<OracleParameter>
            {
                new OracleParameter("V_FILEID",OracleDbType.Int32) { Value = fileID },
                new OracleParameter("V_USERID",OracleDbType.NVarchar2){Value = userID}
            };

            OracleParameter outPar = new OracleParameter();
            outPar.ParameterName = "V_Result";
            outPar.DbType = DbType.Int32;
            outPar.Size = 400;
            outPar.Direction = ParameterDirection.Output;
            paramArray.Add(outPar);

            return ExecNonQuerySpWithResult("DeleteFileByID", paramArray.ToArray());
        }

        public int DeleteTopic(int topicID, string userID)
        {
            var paramArray = new List<OracleParameter>
            {
                new OracleParameter("V_TOPICID",OracleDbType.Int32) { Value = topicID },
                new OracleParameter("V_UserID",OracleDbType.NVarchar2){Value = userID}
            };

            OracleParameter outPar = new OracleParameter();
            outPar.ParameterName = "V_RESULT";
            outPar.DbType = DbType.Int32;
            outPar.Size = 400;
            outPar.Direction = ParameterDirection.Output;
            paramArray.Add(outPar);

            return ExecNonQuerySpWithResult("DeleteTopicByID", paramArray.ToArray());
        }

        public List<HomeHotItem> GetTopFile(string period, bool isEnglish, int count = 10)
        {
            var startDate = DateTime.MinValue;
            switch (period)
            {
                case "1w":
                    startDate = DateTime.Now.AddDays(-7);
                    break;
                case "2w":
                    startDate = DateTime.Now.AddDays(-14);
                    break;
                case "1m":
                    startDate = DateTime.Now.AddMonths(-1);
                    break;
                case "3m":
                    startDate = DateTime.Now.AddMonths(-3);
                    break;
            }
            var paramArray = new[]
            {
                new OracleParameter("V_STARTDATETIME", OracleDbType.TimeStamp) {Value = (OracleTimeStamp)startDate},
                new OracleParameter("V_count", OracleDbType.Int32) {Value = count},
                new OracleParameter("CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}

            };
            var table = GetDataSetBySp("GetHotFile", paramArray).Tables[0];
            var data = new List<HomeHotItem>();
            var i = 0;
            foreach (DataRow row in table.Rows)
            {
                data.Add(new HomeHotItem
                {
                    ID = row["ID"].ToString(),
                    Name = isEnglish ? row["TITLEEN"].ToString() : row["TITLECN"].ToString(),
                    TopicID = row["TOPICID"].ToString(),
                    Rank = ++i,
                    Hits = row["HITS"] is DBNull ? "0" : row["HITS"].ToString()
                });
            }
            return data;
        }

        public List<HomeHotItem> GetLatestFile(bool isEnglish)
        {
            var paramArray = new[]
            {
                new OracleParameter("CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}

            };
            var table = GetDataSetBySp("GetLatestFile", paramArray).Tables[0];
            var data = new List<HomeHotItem>();
            var i = 0;
            foreach (DataRow row in table.Rows)
            {
                data.Add(new HomeHotItem
                {
                    ID = row["ID"].ToString(),
                    Name = isEnglish ? row["TITLEEN"].ToString() : row["TITLECN"].ToString(),
                    TopicID = row["TOPICID"].ToString(),
                    SubmitDate = row.Field<DateTime>("SUBMITDATE").ToString("yyyy-MM-dd H:mm:ss"),
                    Rank = ++i
                });
            }
            return data;
        }

        public List<string> GetAnnouncement(bool isEnglish)
        {
            var ret = new List<string>();
            var paramArray = new[]
            {
                new OracleParameter("CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}

            };
            var table = GetDataSetBySp("GetAnnouncement", paramArray).Tables[0];
            foreach (DataRow row in table.Rows)
            {
                ret.Add(isEnglish ? row["HTMLCONTENTEN"].ToString() : row["HTMLCONTENTCN"].ToString());
            }
            return ret;
        }

        public List<Tuple<string, List<HomeHotItem>>> GetStaticTopics(bool isEnglish)
        {
            var paramArray = new[]
            {
                new OracleParameter("CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}

            };
            var data = GetDataSetBySp("GetHomeStaticLayout", paramArray).Tables[0];
            var moduleDict = new Dictionary<string, Tuple<string, List<HomeHotItem>>>();
            foreach (DataRow row in data.Rows)
            {
                var mid = row["mid"].ToString();
                if (!moduleDict.ContainsKey(mid))
                {
                    var moduleName = isEnglish ? row["MODULENAMEEN"].ToString() : row["MODULENAMECN"].ToString();
                    moduleDict.Add(mid, new Tuple<string, List<HomeHotItem>>(moduleName, new List<HomeHotItem>()));
                }
                var moduleList = moduleDict[mid].Item2;
                moduleList.Add(new HomeHotItem { ID = row["TID"].ToString(), Name = isEnglish ? row["NAMEEN"].ToString() : row["NAMECN"].ToString() });
            }
            return moduleDict.Values.ToList();
        }

        public void UploadFile(FileCreate fileCreate)
        {
            var paramArray = new List<OracleParameter>
            {
                new OracleParameter("V_ID", OracleDbType.Int64) {Value = fileCreate.ID},
                new OracleParameter("V_TitleCn", OracleDbType.NVarchar2) {Value = fileCreate.TitleCn},
                new OracleParameter("V_TitleEn", OracleDbType.NVarchar2) {Value = fileCreate.TitleEn},
                new OracleParameter("V_TopicID", OracleDbType.Int32) {Value = fileCreate.TopicID},
                new OracleParameter("V_Status", OracleDbType.Int32,ParameterDirection.InputOutput) {Value = fileCreate.Status},
                new OracleParameter("V_SubmitDate", OracleDbType.TimeStamp) {Value = (OracleTimeStamp)fileCreate.SubmitDate},
                new OracleParameter("V_ReportDate", OracleDbType.TimeStamp) {Value = (OracleTimeStamp)fileCreate.ReportDate},
                new OracleParameter("V_FileType", OracleDbType.NVarchar2) {Value = fileCreate.FileType},
                //new OracleParameter("V_Doc", OracleDbType.VarBinary) {Value = fileCreate.Doc},
                new OracleParameter("V_RIC", OracleDbType.NVarchar2) {Value = fileCreate.RIC},
                new OracleParameter("V_DescrCn", OracleDbType.NVarchar2) {Value = fileCreate.DescrCn},
                new OracleParameter("V_DescrEn", OracleDbType.NVarchar2) {Value = fileCreate.DescrEn},
                new OracleParameter("V_AuthorRM", OracleDbType.NVarchar2) {Value = fileCreate.AuthorRM},
                new OracleParameter("V_AuthorEmail", OracleDbType.NVarchar2) {Value = fileCreate.AuthorEmail},
                new OracleParameter("V_Author", OracleDbType.NVarchar2) {Value = fileCreate.Author},
                new OracleParameter("V_Tag", OracleDbType.NVarchar2) {Value = fileCreate.Tag},
                new OracleParameter("V_UploadType", OracleDbType.NVarchar2) {Value = fileCreate.UploadType},
                new OracleParameter("V_Source", OracleDbType.NVarchar2) {Value = fileCreate.Source},
                new OracleParameter("V_FileName", OracleDbType.NVarchar2) {Value = fileCreate.FileName},
                new OracleParameter("V_SubmitterID", OracleDbType.NVarchar2) {Value = fileCreate.SubmitterID},
                new OracleParameter("V_DisplayOrder", OracleDbType.NVarchar2) {Value = fileCreate.DisplayOrder},
            };

            OracleParameter outPar = new OracleParameter();
            outPar.ParameterName = "V_Result";
            outPar.DbType = DbType.Int32;
            outPar.Size = 400;
            outPar.Direction = ParameterDirection.Output;
            paramArray.Add(outPar);

            string fileId = "";
            ExecNonQuerySpWithResult("UploadFile", paramArray.ToArray(), out fileId);

            if (!string.IsNullOrEmpty(fileId))
            {
                string path = "";
                using (var db = new IPPEntities())
                {
                    var id = Convert.ToInt32(fileId);
                    var subPath = (from f in db.FILEINFOs
                                   join t in db.TOPICs on f.TOPICID equals t.ID
                                   join m in db.MODULEINFOs on t.MODULEID equals m.ID
                                   where f.ID == id
                                   select new { m.NAMEEN, t.ID }).ToList().FirstOrDefault();
                    path = "/IPP/" + subPath.NAMEEN + "/" + subPath.ID;
                }

                FileService.UploadFile(path, fileId + "." + fileCreate.FileType, fileCreate.Doc);
            }
        }

        public void UploadTopic(TopicCreate topicCreate)
        {
            var paramArray = new[]
                            {
                                new OracleParameter("V_ID", OracleDbType.Int32) { Value = topicCreate.ID},
                                new OracleParameter("V_NameCn", OracleDbType.NVarchar2) { Value = topicCreate.NameCn },
                                new OracleParameter("V_NameEn", OracleDbType.NVarchar2) { Value = topicCreate.NameEn },
                                new OracleParameter("V_ModuleID", OracleDbType.Int32) { Value = topicCreate.ModuleID },
                                new OracleParameter("V_Creater", OracleDbType.NVarchar2) { Value = topicCreate.Creater },
                                //new OracleParameter("V_Image", OracleDbType.VarBinary) { Value = topicCreate.Thumbnail }, 
                                new OracleParameter("V_DescriptionCn", OracleDbType.NVarchar2) { Value = topicCreate.DescriptionCn }, 
                                new OracleParameter("V_DescriptionEn", OracleDbType.NVarchar2) { Value = topicCreate.DescriptionEn} , 
                                new OracleParameter("V_IsApprove",OracleDbType.Int16){Value = topicCreate.IsApprove?1:0},
                                new OracleParameter("V_IsInternalApprove",OracleDbType.Int16){Value = topicCreate.IsInternalApprove?1:0},
                                new OracleParameter("V_IsDirectDelete", OracleDbType.Int16) { Value = topicCreate.IsDirectDelete?1:0 },
                                new OracleParameter("V_Tag", OracleDbType.NVarchar2) { Value = topicCreate.Tag },
                                new OracleParameter("V_RMLink", OracleDbType.NVarchar2) { Value = topicCreate.RMLink },
                                new OracleParameter("V_Approver", OracleDbType.NVarchar2) { Value = topicCreate.Approver },
                                new OracleParameter("V_ImageName", OracleDbType.NVarchar2) { Value = topicCreate.ImageName }
                            };
            ExecNonQuerySp("UploadTopic", paramArray);
        }

        public FileCreate GetFileByFileID(long id)
        {
            var paramArray = new[]
                            {
                                new OracleParameter("V_ID", OracleDbType.Int64) { Value = id},
                                new OracleParameter("CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
                            };
            var table = GetDataSetBySp("GetFileByFileID", paramArray).Tables[0];
            return DataTableSerializer.ToList<FileCreate>(table).FirstOrDefault();
        }

        public byte[] GetFileDataByID(long id)
        {
            var file = FileService.GetFileById(id, "IPP");
            return file == null ? null : file.Content;
        }

        public TopicCreate GetTopicById(int id)
        {
            var paramArray = new[]
                            {
                                new OracleParameter("V_ID", OracleDbType.Int32) { Value = id},
                                new OracleParameter("CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
                            };
            var table = GetDataSetBySp("GetTopicByID", paramArray).Tables[0];
            return DataTableSerializer.ToList<TopicCreate>(table).FirstOrDefault();
        }


        public FILEINFO DownloadFile(long id, string userID)
        {
            var paramArray = new[]
            {
                new OracleParameter("V_ID", OracleDbType.Int64) {Value = id},
                new OracleParameter("V_UserID", OracleDbType.NVarchar2) {Value = userID},
                new OracleParameter("cur", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            var table = GetDataSetBySp("DownloadFile2", paramArray).Tables[0];
            return DataTableSerializer.ToList<FILEINFO>(table).FirstOrDefault();
        }

        public Tuple<bool, bool> GetTopicApporve(int topId)
        {
            using (var IPPDB = new IPPEntities())
            {
                var topic = (from t in IPPDB.TOPICs
                             where t.ID == topId
                             select t).ToList();
                var topicApp = topic.Select(t => new Tuple<bool, bool>(t.ISAPPROVE == 1, t.ISINTERNALAPPROVE == 1)).ToList().FirstOrDefault();

                return topicApp;
            }
        }

        public List<string> GetTopicApprovers(int topId)
        {
            using (var IPPDB = new IPPEntities())
            {
                return (from t in IPPDB.TOPICAPPROVERs
                        where t.TOPICID == topId
                        select t.APPROVER).ToList();
            }
        }

        public List<string> GetTopicApproversByFileId(int fileId)
        {
            using (var IPPDB = new IPPEntities())
            {
                var topicId = (from f in IPPDB.FILEINFOs
                               where f.ID == fileId
                               select f.TOPICID).ToList().FirstOrDefault();

                return GetTopicApprovers((int)topicId);
            }
        }

        public int? GetModuleIdByTopicId(int topId)
        {
            using (var IPPDB = new IPPEntities())
            {
                return (from t in IPPDB.TOPICs
                        where t.ID == topId
                        select t.MODULEID).ToList().FirstOrDefault();
            }
        }

        public List<MODULEINFO> GetModuleList()
        {
            using (var IPPDB = new IPPEntities())
            {
                return (from m in IPPDB.MODULEINFOs
                        select m
                       ).OrderBy(m => m.ID).ToList();
            }
        }

        public List<TOPIC> GetTopicListByModuleId(int moduleId)
        {
            if (moduleId != 0)
            {
                using (var IPPDB = new IPPEntities())
                {
                    return (from m in IPPDB.TOPICs
                            where m.MODULEID == moduleId
                            select m).OrderBy(t => t.ID).ToList();
                }
            }
            else
            {
                using (var IPPDB = new IPPEntities())
                {
                    return (from m in IPPDB.TOPICs
                            select m).OrderBy(t => t.ID).ToList();
                }
            }
        }

        public DataTable GetTopicsByCreater(int moduleID, string userID, int pageNo, int pageSize,out int total)
        {
            var paramArray = new[]
            {
                new OracleParameter("V_ModuleID",OracleDbType.Int32) { Value = moduleID },
                new OracleParameter("V_UserID",OracleDbType.NVarchar2){ Value = userID}, 
                new OracleParameter("V_StartPage",OracleDbType.Int32){ Value = pageNo}, 
                new OracleParameter("V_PageSize",OracleDbType.Int32){ Value = pageSize}, 
                new OracleParameter("O_TOTAL", OracleDbType.Int32){Direction = ParameterDirection.Output},
                new OracleParameter("O_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
                    };
            object outValue;
            var dt = GetDataSetBySp("GetTopicsByCreater", paramArray, "O_TOTAL", out outValue).Tables[0];
            total = string.IsNullOrEmpty(outValue.ToString()) ? 0 : Convert.ToInt32(outValue.ToString());
            return dt;
        }

        public DataTable GetFileRatingStatistics(int fileId)
        {
            var paramArray = new[]
            {
                new OracleParameter("V_fileid",OracleDbType.Int32) { Value = fileId },
                new OracleParameter("CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            return GetDataSetBySp("GetFileRatingStatistics", paramArray).Tables[0];
        }

        public void RateFile(int fileId, string userID, int rate)
        {
            var paramArray = new[]
                            {
                                new OracleParameter("V_fileId", OracleDbType.Int64) { Value = fileId},
                                new OracleParameter("V_UserId", OracleDbType.NVarchar2) { Value = userID},
                                new OracleParameter("V_Rate", OracleDbType.Int32) { Value = rate}
                            };
            ExecNonQuerySp("RateFile", paramArray);
        }

        public int GetUserFileRate(int fileId, string userID)
        {
            var paramArray = new[]
                            {
                                new OracleParameter("V_fileId", OracleDbType.Int64) { Value = fileId},
                                new OracleParameter("V_UserId", OracleDbType.NVarchar2) { Value = userID},
                                new OracleParameter("CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
                            };
            var dataTable = GetDataSetBySp("GetUserFileRating", paramArray).Tables[0];
            return dataTable.Rows.Count == 1 ? Convert.ToInt32(dataTable.Rows[0]["RATE"]) : 5;
        }

        public VAV.Model.Data.IPP.Submitter GeSubmitterById(string id)
        {
            using (var IPPDB = new IPPEntities())
            {
                return (from s in IPPDB.SUBMITTERs
                        where s.ID == id
                        select new VAV.Model.Data.IPP.Submitter { ID = s.ID, Email = s.EMAIL, RM = s.RM, Name = s.NAME }).ToList().FirstOrDefault();
            }
        }

        public void InsertSubmitter(string userId, string email, string name, string rm)
        {
            var paramArray = new[]
                            {
                                new OracleParameter("V_UserID", OracleDbType.NVarchar2) { Value = userId},
                                new OracleParameter("V_Email", OracleDbType.NVarchar2) { Value = email},
                                new OracleParameter("V_Name", OracleDbType.NVarchar2) { Value = name},
                                new OracleParameter("V_RM", OracleDbType.NVarchar2) { Value = rm}
                            };
            ExecNonQuerySp("InsertSubmitter", paramArray);
        }

        private DataSet GetDataSetBySp(string inName, OracleParameter[] inParams)
        {
            using (var IPPDB = new IPPEntities())
            {
                using (var spCmd = new OracleCommand())
                {
                    DataSet ds = null;

                    IPPDB.Database.Connection.Open();
                    spCmd.Connection = new OracleConnection(IPPDB.Database.Connection.ConnectionString);
                    spCmd.CommandText = inName;
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;
                    if (inParams != null)
                    {
                        spCmd.Parameters.AddRange(inParams);
                    }
                    var da = new OracleDataAdapter(spCmd);
                    ds = new DataSet();
                    da.Fill(ds);

                    return ds;
                }
            }
        }


        private DataSet GetDataSetBySp(string inName, OracleParameter[] inParms, string outName, out object outValue)
        {
            using (var IPPDB = new IPPEntities())
            {
                using (var spCmd = new OracleCommand())
                {

                    IPPDB.Database.Connection.Open();
                    spCmd.Connection = new OracleConnection(IPPDB.Database.Connection.ConnectionString);
                    spCmd.CommandText = inName;
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;
                    if (inParms != null)
                        spCmd.Parameters.AddRange(inParms);

                    var da = new OracleDataAdapter(spCmd);
                    var ds = new DataSet();
                    da.Fill(ds);
                    outValue = spCmd.Parameters[outName].Value;
                    return ds;
                }
            }
        }

        protected void ExecNonQuerySp(string inName, OracleParameter[] inParms)
        {
            using (var IPPDB = new IPPEntities())
            {
                IPPDB.Database.Connection.Open();
                DbCommand cmd = IPPDB.Database.Connection.CreateCommand();
                cmd.CommandText = inName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddRange(inParms);
                cmd.ExecuteNonQuery();
            }
        }

        protected int ExecNonQuerySpWithResult(string inName, OracleParameter[] inParms)
        {
            using (var IPPDB = new IPPEntities())
            {
                IPPDB.Database.Connection.Open();
                DbCommand cmd = IPPDB.Database.Connection.CreateCommand();
                cmd.CommandText = inName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddRange(inParms);
                cmd.ExecuteNonQuery();

                var ret = Convert.ToInt32(cmd.Parameters["V_Result"].Value);
                return ret;
            }
        }

        protected void ExecNonQuerySpWithResult(string inName, OracleParameter[] inParms, out string result)
        {
            using (var IPPDB = new IPPEntities())
            {
                IPPDB.Database.Connection.Open();
                DbCommand cmd = IPPDB.Database.Connection.CreateCommand();
                cmd.CommandText = inName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddRange(inParms);
                cmd.ExecuteNonQuery();

                result = Convert.ToString(cmd.Parameters["V_Result"].Value);
            }
        }
    }
}
