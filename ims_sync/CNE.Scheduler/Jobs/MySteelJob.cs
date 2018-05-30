using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using CNE.Scheduler.Extension;
using Luna.DataSync.Core;
using Luna.DataSync.Setting;
using Quartz;
using CNEToolsEntities;
using Oracle.ManagedDataAccess.Client;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;


namespace CNE.Scheduler.Jobs
{
    public class MySteelJob : CmaJobBase
    {

        protected override void ExecuteInternal(JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };
            string connStr = ConfigurationManager.AppSettings["mergeData"];
            var conn = new OracleConnection(connStr);
            conn.Open();
            var tran = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
            var strInfo = new StringBuilder();
            string settingFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                  @"config\MySteel-data-sync.xml");
            var settingManager = new XmlSettingManager(File.ReadAllText(settingFilePath));

            // Set Connection string
            var destinationDbConn = string.Empty;
            settingManager.Init(destinationDbConn);//初始化 mapping

            strInfo.AppendFormat("Source [Type: {0} Address: {1}]\n", settingManager.SourceDb.Type,
                                 settingManager.SourceDb.Conn);
            strInfo.AppendFormat("Destination [Type: {0} Address: {1}]\n", settingManager.DestinationDb.Type,
                                 settingManager.DestinationDb.Conn);

            DateTime lastSyncTime;
            var currentSyncTime = DateTime.Now;
            using (var cmd = new OracleCommand("SELECT max(dLastAccess)  FROM RTMS_TABLEDATA ", conn))
            {
                object obj = cmd.ExecuteScalar();
                lastSyncTime = Convert.ToDateTime(obj.ToString());
                strInfo.AppendFormat("Last successfully sync time : {0}.\n", obj);
            }
            try
            {
                using (var dataSync = new DataSynchronizer(settingManager, lastSyncTime, currentSyncTime))
                {
                    dataSync.TableSynched +=
                        (sender, e) =>
                        strInfo.AppendFormat
                            ("{0} rows have been synchronized from {1} table in SYNC_REUTERS DB to {2} table in CnE DB.\n",
                             e.NumOfRowsSynched, e.Source, e.Dest);

                    dataSync.Init();
                    var sourceTable = settingManager.SourceTableName.Split(',');
                    dataSync.Sync(sourceTable);

                }
                strInfo.AppendFormat("{0} table(s) be synchronized.\n", settingManager.TableMappings.Count());

                var merge = new MergeData();
                merge.ExecuteMetals(conn, tran);
                tran.Commit();

                var endTime = DateTime.UtcNow;
                logEntity.ENDTIME = endTime;
                logEntity.JobStatus = JobStatus.Success;
                logEntity.RUNDETAIL = strInfo.ToString();
                WriteLogEntity(logEntity);
             
            }
            catch (Exception exception)
            {
                tran.Rollback();
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
            get { return "MySteel"; }
        }

        #endregion
    }

}
