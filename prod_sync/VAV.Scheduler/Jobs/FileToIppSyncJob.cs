using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using Luna.DataSync.Core;
using Luna.DataSync.Setting;
using Oracle.ManagedDataAccess.Client;
using Quartz;
using VAVToolsEntities;
using VAV.Scheduler.Util;


namespace VAV.Scheduler.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    public class FileToIppSyncJob : VavJobObject
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
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime, JOBTYPE = JobType};

            var strInfo = new StringBuilder();
            string settingFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                  @"config\File-To-IPP-sync.xml");
            var settingManager = new XmlSettingManager(File.ReadAllText(settingFilePath));

            // Set Connection string
            var destinationDbConn = string.Empty;
            settingManager.Init(destinationDbConn);//初始化 mapping

            strInfo.AppendFormat("<p>Source [Type: {0} Address: {1}]\n", settingManager.SourceDb.Type,
                                 settingManager.SourceDb.Conn);

            strInfo.AppendFormat("Destination [Type: {0} Address: {1}]</p>\n", settingManager.DestinationDb.Type,
                                 settingManager.DestinationDb.Conn);


            var from = getMaxDateTime(settingManager);
            var to = startTime.AddHours(8).AddHours(-settingManager.DeltaHours); 

            strInfo.AppendFormat("<p>Max '{0}' of table '{1}' : {2}.</p>", settingManager.DateKeyColumn, settingManager.DateKeyTable, from);

            strInfo.AppendFormat("<p>Sync duration : {0} to : {1} .</p>", from, to);


            try
            {
                using (var dataSync = new DataSynchronizer(settingManager, from, to))
                {
                    dataSync.TableSynched +=
                        (sender, e) =>
                        strInfo.AppendFormat
                            ("{0} rows have been synchronized from {1} view in CMAFileDB to {2} table in IPP DB.\n",
                             e.NumOfRowsSynched, e.Source, e.Dest);

                    dataSync.PostTaskExecuted += 
                        (sender, e) =>
                        strInfo.AppendFormat
                            ("Post sync task {0} is executed.\n",
                            e.TaskName);

                    dataSync.Init();
                    strInfo.Append(dataSync.Sync(new[] { "GetNewInstitution", "GetNewFile" }));
                }
                
                strInfo.AppendFormat("{0} table(s) be synchronized.\n", settingManager.TableMappings.Count());

                //Update File Topic
                strInfo.AppendFormat("<p>Update File Topic start at {0}.\n", DateTime.UtcNow.ToGMT8String());
                var result = SyncUtil.UpdateFileTopic(from, to);
                strInfo.AppendFormat("Update File Topic completed at {0}.\n Result: {1}</p>", DateTime.UtcNow.ToGMT8String(), result);

                //Rebuild Index
                strInfo.AppendFormat("<p>Rebuild Index at {0}.\n", DateTime.UtcNow.ToGMT8String());
                var result1 = SolrClient.RebuildIndex("full");
                var endTime = DateTime.UtcNow;
                strInfo.AppendFormat("Rebuild Index completed at {0}.\n Result: {1}</p>", DateTime.UtcNow.ToGMT8String(), result);
                if (result1 != "Success")
                {
                    strInfo.AppendFormat("<p style=\"color:red;\">Solr rebuild failed:<br />{0}</p>", result1);
                }

                logEntity.ENDTIME = endTime;
                logEntity.JobStatus = (result == "Success" && result1 == "Success") ? JobStatus.Success : JobStatus.Fail;
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
            var conn = new OracleConnection(setting.DestinationDb.Conn.Replace("Unicode=True;", ""));
            conn.Open();
            var cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "select max(" + setting.DateKeyColumn + ") from " + setting.DateKeyTable ;
            cmd.CommandType = CommandType.Text;
            var dr = cmd.ExecuteReader();
            dr.Read();
            if ((dr[0] == null) || (dr[0] == DBNull.Value))
            {
                var maxDate= Convert.ToDateTime("1999-1-1");
                conn.Dispose();
                return maxDate;
            }
            else
            {
                var maxDate= dr.GetDateTime(0);
                conn.Dispose();
                return maxDate;
            }
        }

        #region Overrides of LunaJobObject

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "FileToIPP_SYNC"; }
        }

        #endregion
    }

}