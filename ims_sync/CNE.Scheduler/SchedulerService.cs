using System;
using System.Diagnostics;
using System.ServiceProcess;
using Quartz;
using Spring.Context;
using Spring.Context.Support;

namespace CNE.Scheduler
{
    /// <summary>
    /// 
    /// </summary>
    public partial class SchedulerService : ServiceBase
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly IScheduler schedulerFactory;

        /// <summary>
        /// 
        /// </summary>
        public SchedulerService()
        {
            InitializeComponent();
            schedulerFactory = CreateSchedulerFactory();
        }

        /// <summary>
        /// Create Scheduler Factory from scheduler.xml
        /// </summary>
        private IScheduler CreateSchedulerFactory()
        {
            try
            {
                IApplicationContext context = ContextRegistry.GetContext();
                return (IScheduler) context.GetObject("CNEScheduler");
            }
            catch (Exception ex)
            {
                this.EventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }
            return null;
        }

        /// <summary>
        /// start scheduler service
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            if (schedulerFactory == null) return;
            try
            {
                schedulerFactory.Start();
            }
            catch (Exception ex)
            {
                this.EventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// Stop scheduler service
        /// </summary>
        protected override void OnStop()
        {
            if (schedulerFactory == null) return;
            try
            {
                schedulerFactory.Shutdown();
            }
            catch (Exception ex)
            {
                this.EventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnPause()
        {
            if (schedulerFactory == null) return;
            try
            {
                schedulerFactory.PauseAll();
            }
            catch (Exception ex)
            {
                this.EventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnContinue()
        {
            if (schedulerFactory == null) return;
            try
            {
                schedulerFactory.ResumeAll();
            }
            catch (Exception ex)
            {
                this.EventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }
        }
    }
}
