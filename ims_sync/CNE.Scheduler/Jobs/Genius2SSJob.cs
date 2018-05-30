using System;
using System.Text;
using System.Linq;
using CNE.Scheduler.Extension;
using Quartz;
using CNEToolsEntities;
using System.Configuration;
using Luna.DataSync.Setting;
using System.IO;
using Luna.DataSync.Core;


namespace CNE.Scheduler.Jobs
{
    public class Genius2SSJob : Ftp2SSJob
    {
        public static bool IsRunningGeniusToFile = false;
        private static readonly object LockObj = new object();

        protected override void ExecuteInternal(JobExecutionContext context)
        {
            lock (LockObj)
            {
                if (IsRunningGeniusToFile)
                    return;
                IsRunningGeniusToFile = true;
            }
         
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime, JOBTYPE = JobType};
            var strInfo = new StringBuilder();
            try
            {
                strInfo = SyncGeniusData(startTime);

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
                    IsRunningGeniusToFile = false;
                }

            }
        }

        private StringBuilder SyncGeniusData(DateTime starTime)
        {
            var strInfo = new StringBuilder();

            var settingFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                  @"config\Genius2SS.xml");
            var settingManager = new XmlSettingManager(File.ReadAllText(settingFilePath));

            var destinationDbConn = string.Empty;
            settingManager.Init(destinationDbConn);//初始化 mapping
            strInfo.AppendFormat("File data Sync begin at {0}\n", starTime);
            strInfo.AppendFormat("Source [Type: {0},  Address: {1}]\n", settingManager.SourceDb.Type, settingManager.SourceDb.Conn);
            strInfo.AppendFormat("Source [Type: FTP, Address: {0}\n", ConfigurationManager.AppSettings["geniusftpUrl"]);
            strInfo.AppendFormat("Destination [Type: Oracle DB,  Address: {0}]\n", ConfigurationManager.AppSettings["reportConnstr"]);
            strInfo.AppendFormat("Destination [Type: WebService, Address: {0}]\n", ConfigHelper.GetEndpointClientAddress("StorageServiceImplPort"));
            var lastSyncTime = GetLastSyncTime().AddHours(8);
            var currentSyncTime = starTime.AddHours(8);

            strInfo.AppendFormat("Last successfully sync time : {0}.\n", lastSyncTime);


            using (var dataSync = new DataSynchronizer(settingManager, lastSyncTime, currentSyncTime))
            {
                dataSync.TableSynched +=
                    (sender, e) =>
                    strInfo.AppendFormat
                        ("{0} rows have been synchronized from {1} table in Genius DB\n",
                         e.NumOfRowsSynched, e.Source);

                dataSync.PostTaskExecuted += (sender, e) =>strInfo.AppendFormat("Post sync task {0} is executed.\n",e.TaskName);

                dataSync.Init();
                var ftpUrl = ConfigurationManager.AppSettings["geniusftpUrl"];
                strInfo.Append(dataSync.FileSync(ftpUrl, Ftp2SS));
            }
            strInfo.AppendFormat("{0} table(s) be synchronized.\n", settingManager.TableMappings.Count());
            strInfo.AppendFormat("Synchronization completed at {0}.\n", DateTime.UtcNow);

            return strInfo;
        }

        #region Overrides of LunaJobObject

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "Genius2SS"; }
        }

        #endregion
    }
}
