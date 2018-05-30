using System;
using System.Linq;
using System.Text;
using Quartz;
using System.IO;
using Luna.DataSync.Setting;
using System.Configuration;
using System.Data.EntityClient;
using Luna.DataSync.Core;
using VAVToolsEntities;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;
using VAV.Scheduler.Util;

namespace VAV.Scheduler.Jobs
{
    public class ZCXSyncJob : VavJobObject
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
                                                  @"config\ZCX-data-sync.xml");
            var settingManager = new XmlSettingManager(File.ReadAllText(settingFilePath));

            //user connection in app.config
            settingManager.Init(string.Empty);//初始化 mapping
            strInfo.AppendFormat("ZCX-data-sync.xml data Sync begin at {0}\n", DateTime.Now);
            strInfo.AppendFormat("Source [Type: {0} Address: {1}]\n", settingManager.SourceDb.Type,
                                 settingManager.SourceDb.Conn);
            strInfo.AppendFormat("Destination [Type: {0} Address: {1}]\n", settingManager.DestinationDb.Type,
                                 settingManager.DestinationDb.Conn);

            var lastSyncTime = new DateTime(1900, 1, 1);
            var currentSyncTime = startTime;

            using (var vavEntities = new VAVEntities())
            {
                var date =
                    vavEntities.SCHEDULERLOGs.Where(x => x.STATUS == 0 && x.JOBTYPE == this.JobType).Select(
                        x => (DateTime?)x.STARTTIME).Max();
                if (date != null)
                    lastSyncTime = date.Value.AddHours(-1.5);
            }
            strInfo.AppendFormat("Last successfully sync time : {0}(System Time).\n", lastSyncTime);

            try
            {
                using (var dataSync = new DataSynchronizer(settingManager, lastSyncTime, currentSyncTime))
                {
                    dataSync.TableSynched +=
                        (sender, e) =>
                        strInfo.AppendFormat
                            ("{0} rows have been synchronized from {1} view in ZCX DB to {2} table in ZCX DB.\n",
                             e.NumOfRowsSynched, e.Source, e.Dest);

                    dataSync.PostTaskExecuted +=
                        (sender, e) =>
                        strInfo.AppendFormat
                            ("Post sync task {0} is executed.\n",
                            e.TaskName);

                    dataSync.Init();

                    var sourceTable = settingManager.SourceTableName.Split(','); 
                    dataSync.Sync(sourceTable);
                }
                strInfo.AppendFormat("{0} table(s) be synchronized.\n", settingManager.TableMappings.Count());
                var endTime = DateTime.UtcNow;
                strInfo.AppendFormat("Synchronization completed at {0}.\n", endTime);
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
            get { return "ZCX_SYNC"; }
        }

        #endregion
    }
}
