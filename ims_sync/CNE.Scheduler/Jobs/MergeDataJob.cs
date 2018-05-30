using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using CNEToolsEntities;
using System.Configuration;
using System.Data.SqlClient;
using CNE.Scheduler.Extension;

namespace CNE.Scheduler.Jobs
{
    public class MergeDataJob : CmaJobBase
    {

        protected override void ExecuteInternal(JobExecutionContext context)
        {
            string connStr = ConfigurationManager.AppSettings["mergeData"].ToString();
            SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            SqlTransaction tran = conn.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();

            strInfo.AppendFormat("ChinaJci data Sync begin at {0}\n", DateTime.UtcNow.ToGmt8String());

            var lastSyncTime = new DateTime(1900, 1, 1);

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




                MergeData merge = new MergeData();
                //merge.Execute(conn, tran);
                tran.Commit();
                #endregion

                var endTime = DateTime.UtcNow;
                strInfo.AppendFormat("Synchronization completed at {0}.\n", endTime.ToGmt8String());
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
        public override string JobType
        {
            get { return "ChinaJci_SYNC"; }
        }
    }
}
