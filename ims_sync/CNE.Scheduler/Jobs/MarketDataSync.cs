using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CNE.Scheduler.Extension;
using CNEToolsEntities;
using Luna.DataSync.Core;
using Luna.DataSync.Setting;
using Quartz;

namespace CNE.Scheduler.Jobs
{
    public class MarketDataSync : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            if (DateTime.Now.Hour < 9 || DateTime.Now.Hour > 18)
            {
                //"Only this time(9:00Am-6:00Pm) point synchronous;";
                return;
            }
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime, JOBTYPE = JobType };
            var strInfo = new StringBuilder();
            string settingFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                  @"config\MarketData-sync.xml");
            var settingManager = new XmlSettingManager(File.ReadAllText(settingFilePath));

            // Set Connection string
            settingManager.Init(string.Empty);//初始化 mapping
            strInfo.AppendFormat("MarketData-sync.xml data Sync begin at {0}(System Time)\n", DateTime.Now);
            strInfo.AppendFormat("Source [Type: {0} Address: {1}]\n", settingManager.SourceDb.Type,
                                 settingManager.SourceDb.Conn);
            strInfo.AppendFormat("Destination [Type: {0} Address: {1}]\n", settingManager.DestinationDb.Type,
                                 settingManager.DestinationDb.Conn);

            var mtime = OracleHelper.GetSingle("SELECT max(MODIFYDATE) FROM ejvasset");
            var lastSyncTime = mtime == null ? Convert.ToDateTime("1999-1-1") : Convert.ToDateTime(mtime);
             
            var currentSyncTime = DateTime.Now;
            try
            {
                using (var dataSync = new DataSynchronizer(settingManager, lastSyncTime, currentSyncTime))
                {
                    dataSync.TableSynched +=
                        (sender, e) =>
                        strInfo.AppendFormat
                            ("{0} rows have been synchronized from {1} to {2}.\n",
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
                var endTime = DateTime.Now;
                strInfo.AppendFormat("Synchronization completed at {0}.\n", endTime);
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
            }
        }

        #region Overrides of LunaJobObject

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "MarketData_Sync"; }
        }

        #endregion
    }
}
