using System;
using System.Data.SqlClient;
using System.Text;
using System.Linq;
using Quartz;
using CNEToolsEntities;
using CNE.Scheduler.Extension;
using System.Configuration;
using Luna.DataSync.Setting;
using System.IO;
using Luna.DataSync.Core;


namespace CNE.Scheduler.Jobs
{
    public class DM12DM2Job : CmaJobBase
    {
        public static bool IsRunningDM12DM2 = false;
        private static readonly object LockObj = new object();

        protected override void ExecuteInternal(JobExecutionContext context)
        {
            lock (LockObj)
            {
                if (IsRunningDM12DM2)
                    return;
                IsRunningDM12DM2 = true;
            }

            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime, JOBTYPE = JobType };
            var strInfo = new StringBuilder();
            try
            {
                strInfo = SyncFileDetailData(startTime);

                logEntity.JobStatus = JobStatus.Success;
                logEntity.RUNDETAIL = strInfo.ToString();

            }
            catch (Exception e)
            {
                logEntity.JobStatus = JobStatus.Fail;
                logEntity.RUNDETAIL = strInfo + "\n" + e;
            }
            finally
            {
                logEntity.ENDTIME = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(logEntity.RUNDETAIL))
                    WriteLogEntity(logEntity);
                lock (LockObj)
                {
                    IsRunningDM12DM2 = false;
                }

            }
        }

        protected StringBuilder SyncFileDetailData(DateTime startTime)
        {
            var strInfo = new StringBuilder();

            var sourceConnStr = ConfigurationManager.AppSettings["CnEConFileData"];
            strInfo.AppendFormat("DM1 File data Synchronization begin at {0}\n", startTime);
            strInfo.AppendFormat("Source [Type: SQLSERVER DB,  Address: {0}]\n", sourceConnStr);
            strInfo.AppendFormat("Destination [Type: Oracle DB,  Address: {0}]\n", ConfigurationManager.AppSettings["reportConnstr"]);
            strInfo.AppendFormat("Destination [Type: WebService, Address: {0}]\n\n", ConfigHelper.GetEndpointClientAddress("StorageServiceImplPort"));

            var lastSyncTime = GetLastSyncTime();
            //string sqlDate = "SELECT MAX(MTIME) FROM FILEDETAIL where  OPERATORS is not null ";
            //var lastSyncTime = Convert.ToDateTime(DBHelper.ExecuteScaler(sqlDate).ToString());

            var currentSyncTime = startTime;

            var successCount = 0;
            using (var sqlCon = new SqlConnection(sourceConnStr))
            {
                sqlCon.Open();
                var sql = "select f.[FileId], f.[FileTypeCode], f.[FileNameCn], f.[FileNameEn], f.[UploadDate], f.[Author], f.[ReportDate], f.[Operator], " +
                          "f.[IsValid], f.[Extension], f.[InstitutionInfoCode], f.[FileSize], f.[FileOrder], f.[CTIME], f.[MTIME], f.[businessType], d.[Content] " +
                          "from [dbo].[FileDetail] f inner join [dbo].[FileData] d on f.[FileId] = d.[FileId] " +
                          "where f.[MTIME] between '{0}' and '{1}' and f.[Operator] is not null and operator <> ''";
                sql = string.Format(sql, lastSyncTime, currentSyncTime);
                strInfo.AppendFormat("144 ExecuteSQL:{0}.\n", sql);
                var sqlCmd = new SqlCommand(sql, sqlCon);
                sqlCmd.CommandTimeout = 600;
                using (var reader = sqlCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var reportManager = new ReportManager();
                        var fileDetail = new FileDetail
                        {
                            FileCode = reader["FileTypeCode"].ToString(),
                            FileNameCN = reader["FileNameCn"].ToString(),
                            FileNameEN = reader["FileNameEn"].ToString(),
                            UploadTime = Convert.ToDateTime(reader["UploadDate"]),
                            Author = reader["Author"].ToString(),
                            Operator = reader["Operator"].ToString() ?? "",
                            IsValid = Convert.ToBoolean(reader["IsValid"]) ? 1 : 0,
                            Ext = reader["Extension"].ToString(),
                            InstitutionCode = reader["InstitutionInfoCode"].ToString(),
                            FileSize = reader["FileSize"].ToString(),
                            FileOrder = string.IsNullOrEmpty(reader["FileOrder"].ToString()) ? 0: Convert.ToInt32(reader["FileOrder"].ToString()),
                            CreateTime = reader["CTIME"].ToString(),
                            BusinessCode = reader["businessType"].ToString(),
                            Dm1FileId = Convert.ToInt32(reader["FileId"])
                        };
                        var success = reportManager.SaveResearchReport(fileDetail, (byte[])reader["Content"]);
                        if (success)
                        {
                            strInfo.AppendFormat("{0},FileID:{1}, Sync successfully.\n", fileDetail.FileNameCN, fileDetail.FileId);
                            successCount++;
                        }
                    }
                }
            }
            var endTime = DateTime.UtcNow;
            strInfo.AppendFormat("\nSynchronization completed at {0}, Synchronization Count: {1}.\n", endTime, successCount);

            return strInfo;
        }

        #region Overrides of LunaJobObject

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "DM12DM2"; }
        }

        #endregion
    }
}
