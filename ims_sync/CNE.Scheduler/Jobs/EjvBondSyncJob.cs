using System;
using System.IO;
using System.Text;
using Luna.DataSync.Core;
using Luna.DataSync.Setting;
using Quartz;
using CNEToolsEntities;


namespace CNE.Scheduler.Jobs
{
    public class EjvBondSyncJob : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            if (DateTime.Now.Hour < 9 || DateTime.Now.Hour > 18)
            {
                //"Only this time(9:00Am-6:00Pm) point synchronous;";
                return;
            }
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };
            var strInfo = new StringBuilder();
            string settingFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                  @"config\EjvBond-data-sync.xml");
            var settingManager = new XmlSettingManager(File.ReadAllText(settingFilePath));

            // Set Connection string
            var destinationDbConn = string.Empty;
            settingManager.Init(destinationDbConn);//初始化 mapping

            strInfo.AppendFormat("Source [Type: {0} Address: {1}]\n", settingManager.SourceDb.Type,
                                 settingManager.SourceDb.Conn);
            strInfo.AppendFormat("Destination [Type: {0} Address: {1}]\n", settingManager.DestinationDb.Type,
                                 settingManager.DestinationDb.Conn);

            var lastSyncTime = new DateTime(2017, 02, 21);
            var currentSyncTime = DateTime.Now;
            try
            {
                using (var dataSync = new DataSynchronizer(settingManager, lastSyncTime, currentSyncTime))
                {
                    dataSync.PostTaskExecuted +=
                        (sender, e) =>
                        strInfo.AppendFormat
                            ("Post sync task {0} is executed.\n",
                            e.TaskName);

                    dataSync.Init();
                    var sourceTable = settingManager.SourceTableName.Split(',');
                    strInfo.Append(dataSync.SyncEachTableFromMaxMtime(sourceTable, "lastupdate"));
                    //strInfo.Append(dataSync.Sync(sourceTable));
                }
                var endTime = DateTime.UtcNow;
                logEntity.ENDTIME = endTime;
                logEntity.JobStatus = JobStatus.Success;
                logEntity.RUNDETAIL = strInfo.ToString();
                WriteLogEntity(logEntity);

            }
            catch (Exception exception)
            {
                logEntity.ENDTIME = DateTime.UtcNow.AddDays(-1);
                logEntity.JobStatus = JobStatus.Fail;
                logEntity.RUNDETAIL = strInfo + "\n" + exception;
                WriteLogEntity(logEntity);
            }
        }

        #region Overrides of LunaJobObject

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "EJV_Input"; }
        }

        #endregion
    }

}
