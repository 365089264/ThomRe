using System;
using System.Linq;
using System.Text;
using Quartz;
using CNEToolsEntities;
using CNE.Scheduler.Extension;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;

namespace CNE.Scheduler.Jobs
{
    public class ChinaJciJob : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            var strInfo = new StringBuilder();
            #region 执行数据同步程序
            if (DateTime.Now.Hour <= 15 || DateTime.Now.Hour >= 22)
            {
                strInfo.Append("Only this time(15:00Am-10:00Pm) point synchronous;");
                return;
            }
            #endregion

            string connStr = ConfigurationManager.AppSettings["mergeData"].ToString();
            OracleConnection conn = new OracleConnection(connStr);
            conn.Open();
            OracleTransaction tran = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            

            strInfo.AppendFormat("Destination [Type: Oracle DB,  Address: {0}]\n" + "\r\n", connStr);
            var lastSyncTime = new DateTime(2016, 9, 02);

            using (var cneEntities = new CnEEntities())
            {
                var date =
                    cneEntities.SCHEDULERLOGs.Where(x => x.STATUS == 0 && x.JOBTYPE == JobType).Select(
                        x => (DateTime?)x.STARTTIME).Max();
                if (date != null)
                {
                    //ToGMT8
                    lastSyncTime = date.Value.AddHours(8);
                }
            }
            try
            {
                #region 执行数据同步程序

                var ch = new ChinaJci();
                var sbSync = new StringBuilder();
                ch.SyncChinaJciData(ref sbSync, lastSyncTime.ToString("yyyy-MM-dd"));
                strInfo.AppendFormat(sbSync.ToString());
                #endregion

                MergeData merge = new MergeData();
                merge.Execute(conn, tran);
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
            finally
            {
                conn.Close();
            }


        }
        #region Overrides of LunaJobObject

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "JCIChina"; }
        }

        #endregion
    }
}
