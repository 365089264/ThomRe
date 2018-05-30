using System;
using System.IO;
using System.Linq;
using System.Text;
using Luna.DataSync.Core;
using Luna.DataSync.Setting;
using Quartz;
using CNEToolsEntities;
namespace CNE.Scheduler.Jobs
{
    public class EjvDsosSyncJob : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };
            var strInfo = new StringBuilder();
            string settingFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                  @"config\dsos-increment-sync.xml");
            var settingManager = new XmlSettingManager(File.ReadAllText(settingFilePath));

            // Set Connection string
            var destinationDbConn = string.Empty;
            settingManager.Init(destinationDbConn);//初始化 mapping

            strInfo.AppendFormat("Source [Type: {0} Address: {1}]\n", settingManager.SourceDb.Type,
                                 settingManager.SourceDb.Conn);
            strInfo.AppendFormat("Destination [Type: {0} Address: {1}]\n", settingManager.DestinationDb.Type,
                                 settingManager.DestinationDb.Conn);

            var lastSyncTime = new DateTime(2017, 2, 21);
            var currentSyncTime = DateTime.Now;
            try
            {
                var failedTableCount = 0;
                using (var dataSync = new DataSynchronizer(settingManager, lastSyncTime, currentSyncTime))
                {
                    //dataSync.TableSynched +=
                    //   (sender, e) =>
                    //   strInfo.AppendFormat
                    //       ("{0} rows have been synchronized from {1}  \n",
                    //        e.NumOfRowsSynched, e.Source);
                    dataSync.PostTaskExecuted +=
                        (sender, e) =>
                        strInfo.AppendFormat
                            ("Post sync task {0} is executed.\n",
                            e.TaskName);

                    dataSync.Init();
                    var sourceTable = settingManager.SourceTableName.Split(',');
                    strInfo.Append(dataSync.SyncEachTableFromMaxMtime(sourceTable, ref failedTableCount, "asset_last_chg_dt"));
                    //dataSync.Sync(sourceTable);
                }
                var endTime = DateTime.UtcNow;
                logEntity.ENDTIME = endTime;
                logEntity.JobStatus = failedTableCount > 0 ? JobStatus.Fail : JobStatus.Success;
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
            get { return "EJV_DSOS"; }
        }

        #endregion
    }

}