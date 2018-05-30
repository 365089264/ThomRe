using System;
using Common.Logging;
using Quartz;
using CNE.Scheduler.Jobs;

namespace CNE.Scheduler
{
    /// <summary>
    /// Example job.
    /// </summary>
    public class ExampleJob : CmaJobBase
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly ILog log = LogManager.GetLogger(typeof(ExampleJob));

        private string userName;

        /// <summary>
        /// Simple property that can be injected.
        /// </summary>
        public string UserName
        {
            set { userName = value; }
        }

        /// <summary>
        /// Execute.
        /// </summary>
        /// <param name="context"></param>
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            try
            {
                //var logEntity = new SCHEDULERLOG
                //                    {
                //                        STARTTIME = DateTime.UtcNow,
                //                        EndTime = DateTime.UtcNow,
                //                        JobStatus = JobStatus.Success,
                //                        RunDetail = "Example success!"
                //                    };
                //WriteLogEntity(logEntity);
                log.Debug("Example");
                System.Diagnostics.Debug.WriteLine("Sample job...");
            }
            catch (Exception exception)
            {
                log.Error(exception);
            }
        }

        #region Overrides of LunaJobObject

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "Example"; }
        }
        #endregion
    }
}
