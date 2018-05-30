using System;
using System.Configuration;
using System.Data.EntityClient;
using System.IO;
using System.Linq;
using System.Text;
using Luna.DataSync.Core;
using Luna.DataSync.Setting;
using Quartz;
using VAVToolsEntities;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;
using Common.Logging;
using VAV.Scheduler.Util;

namespace VAV.Scheduler.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    public class DsosEJVSyncJob : VavJobObject
    {

        /// <summary>
        /// Execute the actual job. The job data map will already have been
        ///             applied as object property values by execute. The contract is
        ///             exactly the same as for the standard Quartz execute method.
        /// </summary>
        /// <seealso cref="M:Spring.Scheduling.Quartz.QuartzJobObject.Execute(Quartz.JobExecutionContext)"/>
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime, JOBTYPE = JobType };

            var strInfo = new StringBuilder();
            string settingFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                  @"config\dsos-new-increment-sync.xml");
            var settingManager = new XmlSettingManager(File.ReadAllText(settingFilePath));
            // Set Connection string
            //var entityConnectionString =
            //    ConfigurationManager.ConnectionStrings["VAVEntities"].ConnectionString;
            //var connectionStringBuilder = new EntityConnectionStringBuilder(entityConnectionString);
            //var DestinationDbConn = connectionStringBuilder.ProviderConnectionString;


            settingManager.Init(string.Empty);//初始化 mapping
            strInfo.AppendFormat("DSOS Sync begin at {0}\n", startTime);
            strInfo.AppendFormat("Source [Type: {0} Address: {1}]\n", settingManager.SourceDb.Type,
                                 settingManager.SourceDb.Conn);

            strInfo.AppendFormat("Destination [Type: {0} Address: {1}]\n", settingManager.DestinationDb.Type,
                                 settingManager.DestinationDb.Conn);

            // Get Last successfully date time.
            var lastSyncTime = new DateTime(2015, 3, 1);

            using (var vavEntities = new VAVEntities())
            {
                var date =
                    vavEntities.SCHEDULERLOGs.Where(x => x.STATUS == 0 && x.JOBTYPE == this.JobType).Select(
                        x => (DateTime?)x.STARTTIME).Max();
                if (date != null)
                {
                    lastSyncTime = date.Value;
                    //Add Buffer
                    lastSyncTime = lastSyncTime.AddHours(-1.5);
                }
            }
            strInfo.AppendFormat("Last successfully sync time : {0}(Eastern Standard Time).\n", startTime);

            try
            {
                List<string> tables;

                //if (hour >= 12 && hour <= 14) //sync iss_def
                //{
                //    //Delete iss_def
                //    strInfo.AppendFormat("Delete iss_def at {0}.\n", DateTime.UtcNow.ToGMT8String());
                //    var re = SyncUtil.Delete_Iss_Def();
                //    strInfo.AppendFormat("Delete iss_def at {0}.\n Result: {1}", DateTime.UtcNow.ToGMT8String(), re);

                //    tables = GetDsosSrcTablesToSync().ToList();
                //}
                //else
                //{
                //    tables = GetDsosSrcTablesToSync().ToList();
                //    var s = tables.Single(t => t == "govcorp..iss_def");
                //    tables.Remove(s);
                //}
                tables = settingManager.TableMappings.Select(t => t.Source).ToList();
                using (var dataSync = new DataSynchronizer(settingManager, lastSyncTime, startTime))
                {
                    dataSync.TableSynched +=
                        (sender, e) =>
                        strInfo.AppendFormat
                            ("{0} rows have been synchronized from {1} view in DSOS to {2} table in VAV DB.\n",
                             e.NumOfRowsSynched, e.Source, e.Dest);

                    dataSync.PostTaskExecuted +=
                        (sender, e) =>
                        strInfo.AppendFormat
                            ("Post sync task {0} is executed.\n",
                            e.TaskName);

                    dataSync.Init();
                    dataSync.Sync(tables);
                }
                strInfo.AppendFormat("{0} table(s) be synchronized.\n", settingManager.TableMappings.Count());
                strInfo.AppendFormat("Synchronization completed at {0}.\n", DateTime.UtcNow.ToGMT8String());

                //Update Bond Info
                strInfo.AppendFormat("Update Bond Info en&cn start at {0}.\n", DateTime.UtcNow.ToGMT8String());
                //var result = SyncUtil.UpdateBondInfo(lastSyncTime, currentSyncTime);
                var endTime = DateTime.UtcNow;
                //strInfo.AppendFormat("Update Bond Info en&cn completed at {0}.\n Result: {1}", DateTime.UtcNow.ToGMT8String(), result);

                ////Rebuild Index
                //strInfo.AppendFormat("Rebuild Index at {0}.\n", DateTime.UtcNow.ToGMT8String());
                //var result1 = SolrClient.RebuildIndex("full");
                //strInfo.AppendFormat("Rebuild Index completed at {0}.\n Result: {1}", DateTime.UtcNow.ToGMT8String(), result1);

                logEntity.ENDTIME = endTime;
                //logEntity.JobStatus = (result == "Success" && result1 == "Success") ? JobStatus.Success : JobStatus.Fail;
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
            get { return "DSOSEJV_SYNC"; }
        }

        #endregion
    }

}