using System;
using System.Text;
using Quartz;
using CNEToolsEntities;
using CNE.Scheduler.Extension;
using System.Configuration;

namespace CNE.Scheduler.Jobs
{
    public class OpenMarketData : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();

            strInfo.AppendFormat("<p>Source [Type: RSSL Host: 10.35.30.44:14002 ServiceName:ELEKTRON_DD]\n");

            strInfo.AppendFormat("Destination [Type: {0} Address: {1}]</p>", "Oracle",
                                 ConfigurationManager.AppSettings["MarketCon"]);


            StarterConsumer starterConsumer = new StarterConsumer();
            try
            {

                #region 执行数据同步程序
                if (DateTime.Now.Hour < 9 || DateTime.Now.Hour > 18)
                {
                    strInfo.Append("Only this time(9:00Am-6:00Pm) point synchronous;");
                    return;
                }
                logEntity.JobStatus = starterConsumer.Run(strInfo);

                #endregion

                var endTime = DateTime.UtcNow;
                logEntity.ENDTIME = endTime;
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
            finally
            {
                starterConsumer.Cleanup();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "RFA_OpenMarket"; }
        }
    }
}
