using System;
using System.Text;
using CNE.Scheduler.Extension;
using Quartz;
using CNEToolsEntities;


namespace CNE.Scheduler.Jobs
{
    public class LufaxJob : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();

            strInfo.AppendFormat("Lufax  Sync begin at {0}\n", DateTime.UtcNow.ToGmt8String());
            try
            {
                #region 执行数据同步程序
                var fn = new Lufax();
                fn.GetFtpData(strInfo);
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
                logEntity.ENDTIME = DateTime.UtcNow.AddDays(-1);
                logEntity.JobStatus = JobStatus.Fail;
                logEntity.RUNDETAIL = strInfo + "\n" + exception;
                WriteLogEntity(logEntity);
            }
        }

        #region Overrides of CNEJobObject

        public override string JobType
        {
            get { return "Lufax_SYNC"; }
        }

        #endregion
    }
}
