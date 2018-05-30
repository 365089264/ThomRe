using System;
using System.Text;
using Quartz;
using CNEToolsEntities;
using CNE.Scheduler.Extension;
using System.Configuration;

namespace CNE.Scheduler.Jobs
{
    public class FanyaJob : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();

            strInfo.AppendFormat("Fanya data Sync begin at {0}\n", DateTime.UtcNow.ToGmt8String());
            var htmlAddress = ConfigurationManager.AppSettings["CnEFanyaUrl"];
            strInfo.AppendFormat("Source [Type: {0} Address: {1}]\n", "html",
                                 htmlAddress);

            strInfo.AppendFormat("Destination [Type: {0} Address: {1}]\n", "SqlServer",
                                 ConfigurationManager.AppSettings["CnECon"]);


            try
            {
                #region 执行数据同步程序

                var mf = new MetalsFanya();
                mf.Updatefanya(htmlAddress);

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

        #region Overrides of LunaJobObject

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "Fanya_SYNC"; }
        }

        #endregion
    }
}
