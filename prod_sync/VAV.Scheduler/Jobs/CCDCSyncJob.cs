using System;
using System.Text;
using Quartz;
using System.IO;
using System.Linq;
using Luna.DataSync.Setting;
using Luna.DataSync.Core;
using VAVToolsEntities;

namespace VAV.Scheduler.Jobs
{
    public class CCDCSyncJob : VavJobObject
    {

        /// <summary>
        /// Sync data from OpenMarketDB and hosted in .144 which is the transfer DB
        /// </summary>
        /// <seealso cref="M:Spring.Scheduling.Quartz.QuartzJobObject.Execute(Quartz.JobExecutionContext)"/>
        protected override void ExecuteInternal(JobExecutionContext context)
        {

            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();
            string settingFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                  @"config\CCDC-data-sync.xml");
            var settingManager = new XmlSettingManager(File.ReadAllText(settingFilePath));

            // Set Connection string
            settingManager.Init(string.Empty);//初始化 mapping
            strInfo.AppendFormat("Source [Type: {0} Address: {1}]\n", settingManager.SourceDb.Type,
                                 settingManager.SourceDb.Conn);
            strInfo.AppendFormat("Destination [Type: {0} Address: {1}]\n", settingManager.DestinationDb.Type,
                                 settingManager.DestinationDb.Conn);

            var lastSyncTime = new DateTime(2016, 1, 1);
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
                    var sourceTable = settingManager.TableMappings.Select(t => t.Source).ToList();
                    strInfo.Append(dataSync.SyncEachTableFromMaxMtime(sourceTable, "LAST_UPDATE"));
                }
                var endTime = DateTime.UtcNow;
                logEntity.ENDTIME = endTime;
                logEntity.JobStatus = JobStatus.Success;
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
            get { return "CCDC_SYNC"; }
        }

        #endregion
    }
}
