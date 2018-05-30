using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CNEToolsEntities;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace CNE.Scheduler.Jobs
{
    public class ClearHistoryDataJob : CmaJobBase
    {
        public override string JobType
        {
            get { return "ClearHistoryDataJob"; }
        }

        protected override void ExecuteInternal(Quartz.JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();

            strInfo.AppendFormat("ClearHistoryDataJob  begin at {0}\n", DateTime.UtcNow.ToGmt8String());

            using (SqlConnection conn = new SqlConnection("server=10.35.63.144;database=CnE;uid=sa;password=p@ssw0rd"))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    #region 执行数据同步程序


                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Transaction = tran;
                        cmd.CommandText = "[dbo].[CLEARSHIPPINGDATA]";
                        cmd.ExecuteNonQuery();
                    }
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Transaction = tran;
                        cmd.CommandText = "[dbo].[CLEARFANTAMETALDATA]";
                        cmd.ExecuteNonQuery();
                    }
                    #endregion
                    tran.Commit();
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
        }
    }
}
