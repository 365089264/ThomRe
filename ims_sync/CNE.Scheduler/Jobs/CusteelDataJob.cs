using System;
using System.Configuration;
using System.Text;
using Quartz;
using CNEToolsEntities;
using CNE.Scheduler.Extension;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;

namespace CNE.Scheduler.Jobs
{
    public class CusteelDataJob : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();

            string connStr = ConfigurationManager.AppSettings["mergeData"];
            strInfo.AppendFormat("Source [Type: {0} Address: {1}]\n", "CusteelService",
                                 "http://db.custeel.com/services/dataCenterMTOMVerifyService");
            strInfo.AppendFormat("Destination [Type: {0} Address: {1}]\n", "Oracle",
                                 connStr);
            var conn = new OracleConnection(connStr);
            conn.Open();
            var tran = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
            try
            {
                #region 执行数据同步程序
                var ch = new CusteelData();
                ch.SyncCusteelData(strInfo);
                #endregion
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
            get { return "CusteelPriceData"; }
        }

        #endregion
    }
}
