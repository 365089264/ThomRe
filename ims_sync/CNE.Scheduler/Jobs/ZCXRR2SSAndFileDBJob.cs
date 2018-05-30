using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using Luna.DataSync.Core;
using Luna.DataSync.Setting;
using Quartz;
using CNEToolsEntities;
using CNE.Scheduler.Extension;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using CNE.Scheduler.HTML_PDF_Tool;
using System.Data;

namespace CNE.Scheduler.Jobs
{

    public class ZCXRR2SSAndFileDBJob : Ftp2SSJob
    {
        public static bool IsRunningZCXToFile = false;
        private static readonly object LockObj = new object();
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            lock (LockObj)
            {
                if (IsRunningZCXToFile)
                    return;
                IsRunningZCXToFile = true;
            }

            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };
            var strInfo = new StringBuilder("log: \n");
            try
            {
                strInfo = ZCXRRDataSync(startTime);

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
                WriteLogEntity(logEntity);
                lock (LockObj)
                {
                    IsRunningZCXToFile = false;
                }

            }
        }

        private StringBuilder ZCXRRDataSync(DateTime starTime)
        {
            var strInfo = new StringBuilder();

            var settingFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                  @"config\ZCXRR2SSAndFileDB.xml");
            var settingManager = new XmlSettingManager(File.ReadAllText(settingFilePath));

            var destinationDbConn = string.Empty;
            settingManager.Init(destinationDbConn);//初始化 mapping
            strInfo.AppendFormat("File data Sync begin at {0}\n", starTime);
            strInfo.AppendFormat("Source [Type: {0}]\n", settingManager.SourceDb.Type);
            strInfo.AppendFormat("Source [Type: FTP, Address: {0}\n", ConfigurationManager.AppSettings["zcxftpUrl"]);
            strInfo.AppendFormat("Destination [Type: Oracle DB,  Address: {1}]\n", ConfigurationManager.AppSettings["reportConnstr"]);
            strInfo.AppendFormat("Destination [Type: WebService, Address: {0}]\n", ConfigHelper.GetEndpointClientAddress("StorageServiceImplPort"));
            var lastSyncTime = GetLastSyncTime();
            var currentSyncTime = starTime;

            strInfo.AppendFormat("Last successfully sync time : {0}.\n", lastSyncTime);


            using (var dataSync = new DataSynchronizer(settingManager, lastSyncTime, currentSyncTime))
            {
                dataSync.TableSynched +=
                    (sender, e) =>
                    strInfo.AppendFormat
                        ("{0} rows have been synchronized from {1} table in ZCX DB\n",
                         e.NumOfRowsSynched, e.Source);

                dataSync.PostTaskExecuted += (sender, e) => strInfo.AppendFormat("Post sync task {0} is executed.\n", e.TaskName);

                dataSync.Init();
                var ftpUrl = ConfigurationManager.AppSettings["zcxftpUrl"];
                strInfo.Append(dataSync.FileSync(ftpUrl, Ftp2SS));
            }
            strInfo.AppendFormat("{0} table(s) be synchronized.\n", settingManager.TableMappings.Count());
            strInfo.AppendFormat("Synchronization completed at {0}.\n", DateTime.UtcNow);

            return strInfo;
        }


        //private void bak()
        //{
        //    var startTime = DateTime.UtcNow;
        //    var logEntity = new SCHEDULERLOG { STARTTIME = startTime };
        //    var strInfo = new StringBuilder();
        //    var rootPath = "C:/DataFeedApp/ZCXClient/ZCX/ADD_DATA";

        //    var lastSyncTime = GetLastSyncTime();

        //    string connStr = ConfigurationManager.AppSettings["ZCXDBConnStr"].ToString();
        //    SqlConnection conn = new SqlConnection(connStr);
        //    conn.Open();

        //    strInfo.AppendFormat("Job begin at {0}\n", DateTime.UtcNow.ToGmt8String());
        //    try
        //    {
        //        #region 执行数据同步程序

        //        //loop the files 
        //        var sql = "SELECT * FROM RES_INFO WHERE RES_TYPE_PAR=1 and res_writ_date >'2014-01-01' and CCXEID > '" +
        //                  lastSyncTime.ToString() + "' order by ccxeid asc ";
        //        DataTable tb = new DataTable();
        //        using (SqlCommand cmd = new SqlCommand(sql, conn))
        //        {
        //            SqlDataAdapter sda = new SqlDataAdapter(cmd);
        //            sda.Fill(tb);
        //            sda.Dispose();
        //        }
        //        foreach (DataRow item in tb.Rows)
        //        {
        //            var filePath = rootPath + item["RES_FILE_PATH"].ToString();
        //            var author = item["RESER_NAME"].ToString();
        //            var title = item["RES_TITLE"].ToString();
        //            var utime = item["RES_PUBL_DATE"].ToString();
        //            var ch = new ReportManager();
        //            ch.InsertReportFile(filePath, "zcx", "FI", "RR", author, title, utime, strInfo);
        //        }


        //        #endregion

        //        var endTime = DateTime.UtcNow;
        //        strInfo.AppendFormat("Synchronization completed at {0}.\n", endTime.ToGmt8String());
        //        logEntity.ENDTIME = endTime;
        //        logEntity.JobStatus = JobStatus.Success;
        //        logEntity.RUNDETAIL = strInfo.ToString();
        //        WriteLogEntity(logEntity);

        //    }
        //    catch (Exception exception)
        //    {
        //        logEntity.ENDTIME = DateTime.UtcNow.AddDays(-1);
        //        logEntity.JobStatus = JobStatus.Fail;
        //        logEntity.RUNDETAIL = strInfo + "\n" + exception;
        //        WriteLogEntity(logEntity);
        //    }
        //    finally
        //    {

        //        conn.Close();
        //    }
        //}

        #region Overrides
        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "ZCXRR2SSAndFileDB"; }
        }

        #endregion


    }
}
