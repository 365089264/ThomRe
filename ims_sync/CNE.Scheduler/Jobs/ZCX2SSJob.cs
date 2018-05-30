using System;
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
    public class ZCX2SSJob : Ftp2SSJob
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
                strInfo = SyncZCXData(startTime);

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

        private StringBuilder SyncZCXData(DateTime starTime)
        {
            var strInfo = new StringBuilder();
            string settingFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                  @"config\ZCX2SS.xml");
            var settingManager = new XmlSettingManager(File.ReadAllText(settingFilePath));

            var destinationDbConn = string.Empty;
            settingManager.Init(destinationDbConn);//初始化 mapping
            strInfo.AppendFormat("File data Sync begin at {0}\n", starTime);
            strInfo.AppendFormat("Source [Type: {0},  Address: {1}]\n", settingManager.SourceDb.Type, settingManager.SourceDb.Conn);
            strInfo.AppendFormat("Source [Type: FTP, Address: {0}]\n", ConfigurationManager.AppSettings["zcxftpUrl"]);
            strInfo.AppendFormat("Destination [Type: Oracle DB,  Address: {0}]\n", ConfigurationManager.AppSettings["reportConnstr"]);
            strInfo.AppendFormat("Destination [Type: WebService, Address: {0}]\n", ConfigHelper.GetEndpointClientAddress("StorageServiceImplPort"));

            var lastSyncTime = GetLastSyncTime();
            //string sql = "SELECT MAX(CCXEID) FROM RATE_REP_DATA";
            //var lastSyncTime = Convert.ToDateTime(DBHelper.ExecuteScaler(sql).ToString());

            var currentSyncTime = starTime;

            strInfo.AppendFormat("Last successfully sync time : {0}(Eastern Standard Time).\n", lastSyncTime);

            using (var dataSync = new DataSynchronizer(settingManager, lastSyncTime, currentSyncTime))
            {
                dataSync.TableSynched +=
                    (sender, e) =>
                    strInfo.AppendFormat
                        ("{0} rows have been synchronized from {1} view in ZCX DB \n",
                         e.NumOfRowsSynched, e.Source);

                dataSync.PostTaskExecuted +=
                    (sender, e) =>
                    strInfo.AppendFormat
                        ("Post sync task {0} is executed.\n",
                        e.TaskName);

                dataSync.Init();
                var ftpUrl = ConfigurationManager.AppSettings["zcxftpUrl"];
                strInfo.Append(dataSync.FileSync(ftpUrl, Ftp2SS));
            }
            strInfo.AppendFormat("{0} table(s) be synchronized.\n", settingManager.TableMappings.Count());

            return strInfo;

        }

        #region Overrides of LunaJobObject

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "ZCX2SS"; }
        }

        #endregion
    }
}
