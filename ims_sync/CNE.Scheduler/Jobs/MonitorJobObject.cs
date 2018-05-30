using System;
using Common.Logging;
using Spring.Scheduling.Quartz;
using CNEToolsEntities;


namespace CNE.Scheduler.Jobs
{
    public abstract class MonitorJobObject : QuartzJobObject
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly ILog _log = LogManager.GetLogger("MonitorScheduler");

        /// <summary>
        /// 
        /// </summary>
        public abstract string JobType { get; }

        /// <summary>
        /// 
        /// </summary>
        public void WriteLogEntity(SCHEDULERLOG logEntity)
        {
            using (var cneEntities = new CnEEntities())
            {
                logEntity.JOBTYPE = JobType;
                cneEntities.SCHEDULERLOGs.Add(logEntity);
                cneEntities.SaveChanges();
            }
            var detail = logEntity.RUNDETAIL;
            if (!String.IsNullOrEmpty(detail))
                detail = detail.Replace("\n", "<br/>");
            var str =
                String.Format("<Html><Body style='font-family:Verdana;font-size:12'><h2 style='color:{5}'>[{0}] Scheduler job \"<b>{1}</b>\" finished.</h2><hr/><p>Job Type:{1}</p><p>Start Time:{2}</p><p>End Time:{3}</p><p><b>Detail:</b></p><p>{4}</p></Body></Html>",
                    logEntity.JobStatus, logEntity.JOBTYPE,
                    logEntity.STARTTIME.ToGmt8String(),
                    logEntity.ENDTIME.ToGmt8String(), detail,
                    logEntity.JobStatus == JobStatus.Success ? "green" : "red");
            if (logEntity.JobStatus == JobStatus.Success)
                _log.Info(str);
            else
                _log.Error(str);
        }
    }
}
