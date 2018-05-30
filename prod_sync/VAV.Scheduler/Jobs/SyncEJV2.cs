using System;
using System.Linq;
using System.Text;
using Quartz;
using System.IO;
using Luna.DataSync.Setting;
using Luna.DataSync.Core;
using VAV.Scheduler.Util;
using VAVToolsEntities;

namespace VAV.Scheduler.Jobs
{
    public class SyncEJV2 : VavJobObject
    {
        /// <summary>
        /// Sync data from GeniusDB and hosted in .144 which is the transfer DB
        /// </summary>
        /// <seealso cref="M:Spring.Scheduling.Quartz.QuartzJobObject.Execute(Quartz.JobExecutionContext)"/>
        protected override void ExecuteInternal(JobExecutionContext context)
        {

            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();
            string settingFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                  @"config\EJVMigration2.xml");
            var settingManager = new XmlSettingManager(File.ReadAllText(settingFilePath));

            // Set Connection string
            settingManager.Init(string.Empty);//初始化 mapping
            strInfo.AppendFormat("Source [Type: {0} Address: {1}]\n", settingManager.SourceDb.Type,
                                 settingManager.SourceDb.Conn);
            strInfo.AppendFormat("Destination [Type: {0} Address: {1}]\n", settingManager.DestinationDb.Type,
                                 settingManager.DestinationDb.Conn);

            var lastSyncTime = new DateTime(2017, 3, 17);
            var currentSyncTime = startTime.AddHours(8);

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
                    strInfo.Append(dataSync.SyncEachTableFromMaxMtime(sourceTable, "asset_last_chg_dt"));
                    //strInfo.Append(dataSync.Sync(sourceTable));
                }

                var endTime = DateTime.UtcNow;

                //Update Bond Info
                strInfo.AppendFormat("Update Bond Info en&cn start at {0}.\n", DateTime.UtcNow.ToGMT8String());
                SyncUtil.UpdateBondInfo(lastSyncTime, currentSyncTime);
                strInfo.AppendFormat("Update Bond Info en&cn completed at {0}.\n ", DateTime.UtcNow.ToGMT8String());

                //Rebuild Index
                strInfo.AppendFormat("Rebuild Index at {0}.\n", DateTime.UtcNow.ToGMT8String());
                var result1 = SolrClient.RebuildIndex("full");
                endTime = DateTime.UtcNow;
                strInfo.AppendFormat("Rebuild Index completed at {0}.\n Result: {1}\n", DateTime.UtcNow.ToGMT8String(), result1);
                
                
                logEntity.ENDTIME = endTime;
                logEntity.JobStatus = (result1 == "Success") ? JobStatus.Success : JobStatus.Fail;
                logEntity.RUNDETAIL = strInfo.ToString();
                WriteLogEntity(logEntity);
            }
            catch (Exception exception)
            {
                logEntity.ENDTIME = DateTime.UtcNow;
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
