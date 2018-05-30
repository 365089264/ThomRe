using System;
using System.IO;
using System.Text;
using CNE.Scheduler.Extension;
using Quartz;
using CNEToolsEntities;


namespace CNE.Scheduler.Jobs
{
    public class MonitorJob : MonitorJobObject
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();

            strInfo.AppendFormat("Monitor  begin at {0}\n", DateTime.UtcNow.ToGmt8String());
            try
            {
                #region 执行数据同步程序
                string xmlpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                  @"config\Monitor.xml");
                var monitorSync = new MonitorSync(strInfo, xmlpath,DateTime.Now.ToString("yyyy-MM-dd"));
                monitorSync.Excute();
                #endregion

                var endTime = DateTime.UtcNow;
                strInfo.AppendFormat("Monitor completed at {0}.\n", endTime.ToGmt8String());
                logEntity.ENDTIME = endTime;
                logEntity.JobStatus = JobStatus.Success;
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
        }

        #region Overrides of LunaJobObject

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "Monitor_SYNC"; }
        }

        #endregion
    }

}
