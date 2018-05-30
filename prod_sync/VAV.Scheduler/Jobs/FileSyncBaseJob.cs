using System;
using System.Data;
using System.Data.OracleClient;
using System.IO;
using System.Linq;
using System.Text;
using Luna.DataSync.Core;
using Luna.DataSync.Setting;
using Quartz;
using VAV.Scheduler.Util;
using VAVToolsEntities;

namespace VAV.Scheduler.Jobs
{
    public abstract class FileSyncBaseJob : VavJobObject
    {

        public abstract string ConfigFilePath { get; }

        /// <summary>
        /// Sync data from GeniusDB and hosted in .144 which is the transfer DB
        /// </summary>
        /// <seealso cref="M:Spring.Scheduling.Quartz.QuartzJobObject.Execute(Quartz.JobExecutionContext)"/>
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var settingFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFilePath);
            var settingManager = new XmlSettingManager(File.ReadAllText(settingFilePath));

            var destinationDbConn = string.Empty;
            settingManager.Init(destinationDbConn);//初始化 mapping
            //strInfo.AppendFormat("File data Sync begin at {0}\n", startTime.ToGMT8String());
            strInfo.AppendFormat("<p>Source [Type: {0} Address: {1}]\n", settingManager.SourceDb.Type,
                                 settingManager.SourceDb.Conn);
            strInfo.AppendFormat("Destination [Type: {0} Address: {1}]</p>", settingManager.DestinationDb.Type,
                                 settingManager.DestinationDb.Conn);
            strInfo.AppendFormat("<p>Source [Type: {0} Address: {1}]\n", "WebService",
                                 ConfigHelper.GetEndpointClientAddress("StorageServiceImplPort"));
            strInfo.AppendFormat("Destination [Type: {0} Address: {1}]</p>", "WebService",
                                 ConfigHelper.GetEndpointClientAddress("StorageServiceImplPort1"));

            var lastSyncTime = getMaxDateTime(settingManager);




            // from last sync time
            var from = lastSyncTime;
            // to now - 1h
            var to = startTime.AddHours(8).AddHours(-settingManager.DeltaHours);

            strInfo.AppendFormat("<p>Max '{0}' of table '{1}' : {2}.</p>", settingManager.DateKeyColumn, settingManager.DateKeyTable, from);

            strInfo.AppendFormat("<p>Sync duration : {0} to : {1} .</p>", from, to);


            try
            {
                using (var dataSync = new DataSynchronizer(settingManager, from, to))
                {
                    dataSync.TableSynched +=
                        (sender, e) =>
                       LogFileSyncInfo(sender, e);

                    dataSync.PostTaskExecuted +=
                        (sender, e) =>
                        strInfo.AppendFormat
                            ("Post sync task {0} is executed.\n",
                            e.TaskName);

                    dataSync.Init();

                    var sourceTable = settingManager.SourceTableName.Split(',');
                    strInfo.Append(dataSync.Sync(sourceTable));
                }
                strInfo.AppendFormat("{0} table(s) synchronized.\n", settingManager.TableMappings.Count());
                var endTime = DateTime.UtcNow;
                //strInfo.AppendFormat("Synchronization completed at {0}.\n", endTime.ToGMT8String());
                logEntity.ENDTIME = endTime;
                logEntity.JobStatus = JobStatus.Success;
                logEntity.RUNDETAIL = strInfo.ToString();
                WriteLogEntity(logEntity);
            }
            catch (Exception exception)
            {
                logEntity.ENDTIME = DateTime.UtcNow;
                logEntity.JobStatus = JobStatus.Fail;
                logEntity.RUNDETAIL = strInfo + "\n<b>Exception detail:</b>\n" + exception + "\n<p>No tables synchronized.</p>";
                WriteLogEntity(logEntity);
            }
        }


        private DateTime getMaxDateTime(XmlSettingManager setting)
        {
            var conn = new OracleConnection(setting.DestinationDb.Conn);
            conn.Open();
            var cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "select max(" + setting.DateKeyColumn + ") from " + setting.DateKeyTable;
            cmd.CommandType = CommandType.Text;
            OracleDataReader dr = cmd.ExecuteReader();
            dr.Read();
            var maxDate = dr.GetDateTime(0);
            conn.Dispose();
            return maxDate;
        }


        private StringBuilder strInfo = new StringBuilder();

        private void LogFileSyncInfo(object sender, TableSynchedEventArgs e)
        {
            strInfo.AppendFormat
                            ("{0} rows have been synchronized from {1} to {2}.\n",
                             e.NumOfRowsSynched, e.Source, e.Dest);

            if (!string.IsNullOrEmpty(e.SyncInfo))
                strInfo.AppendFormat("File sync info is: \n {0} \n", e.SyncInfo);
        }
    }
}
