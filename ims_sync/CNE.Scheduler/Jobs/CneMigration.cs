using System;
using System.IO;
using System.Linq;
using System.Text;
using Quartz;
using CNEToolsEntities;
using Luna.DataSync.Core;
using Luna.DataSync.Setting;


namespace CNE.Scheduler.Jobs
{
    public class CneMigration : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };
            var strInfo = new StringBuilder();
          

            var lastSyncTime = new DateTime(2016, 8, 22);
            var currentSyncTime = DateTime.Now;

            using (var cneEntities = new CnEEntities())
            {
                var date =
                    cneEntities.SCHEDULERLOGs.Where(x => x.STATUS == 0 && x.JOBTYPE == JobType).Select(
                        x => (DateTime?)x.STARTTIME).Max();
                if (date != null)
                {
                    lastSyncTime = date.Value.AddHours(8);
                    strInfo.AppendFormat("Last successfully sync time : {0}.\n", lastSyncTime);
                    //Add Buffer
                    lastSyncTime = lastSyncTime.AddHours(-1);
                }
            }
           
            try
            {
                
                string settingFilePath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                           @"config\CusteelTo144.xml");
                var settingManager2 = new XmlSettingManager(File.ReadAllText(settingFilePath2));
                var DestinationDbConn2 = string.Empty;
                settingManager2.Init(DestinationDbConn2);//初始化 mapping
                strInfo.AppendFormat("Source [Type: {0} Address: {1}]\n", settingManager2.SourceDb.Type,
                                 settingManager2.SourceDb.Conn);
                strInfo.AppendFormat("Destination [Type: {0} Address: {1}]\n", settingManager2.DestinationDb.Type,
                                     settingManager2.DestinationDb.Conn);
                using (var dataSync = new DataSynchronizer(settingManager2, lastSyncTime, currentSyncTime))
                {
                    dataSync.TableSynched +=
                        (sender, e) =>
                        strInfo.AppendFormat
                            ("{0} rows have been synchronized from {1} view in Cne DB to {2} table in Cne DB.\n",
                             e.NumOfRowsSynched, e.Source, e.Dest);

                    dataSync.PostTaskExecuted +=
                        (sender, e) =>
                        strInfo.AppendFormat
                            ("Post sync task {0} is executed.\n",
                            e.TaskName);

                    dataSync.Init();
                    var sourceTable = settingManager2.SourceTableName.Split(',');
                    dataSync.Sync(sourceTable);
                }

                strInfo.AppendFormat("{0} table(s) be synchronized.\n", settingManager2.TableMappings.Count());
                /***********************/


                var endTime = DateTime.UtcNow;
                strInfo.AppendFormat("Synchronization completed at {0}.\n", endTime.ToGmt8String());
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
            get { return "CneMigration_SYNC"; }
        }

        #endregion
    }
}
