using System;
using System.Text;
using Quartz;
using CNEToolsEntities;
using CNE.Scheduler.Extension;
namespace CNE.Scheduler.Jobs
{
    public class ChinaSecuritiesJob : CmaJobBase
    {
        protected int InsertRows;
        protected override void ExecuteInternal(JobExecutionContext context)
        {

            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();

            try
            {
                #region 执行数据同步程序
                if (DateTime.Now.Hour < 8 || DateTime.Now.Hour >= 20)
                {
                    strInfo.Append("Only this time(8:00Am-8:00Pm) point synchronous;");
                    return;
                }
                else
                {
                    var ch = new ChinaSecurities();
                    ch.SyncData(strInfo);
                }
                #endregion

                var endTime = DateTime.UtcNow;
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

        #region Overrides of LunaJobObject

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "ChinaSecurities_SYNC"; }
        }

        #endregion
    }
}
