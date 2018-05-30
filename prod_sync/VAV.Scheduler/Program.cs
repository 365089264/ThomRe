using System.ServiceProcess;
using VAV.Scheduler.Jobs;

namespace VAV.Scheduler
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var servicesToRun = new ServiceBase[]
                                              {
                                                  new SchedulerService()
                                              };
            ServiceBase.Run(servicesToRun);

            //var test = new Test();
            //test.ShowDialog();

            //var job1 = new DsosEJVSyncJob();
            //job1.Test();

            //var job2 = new CashflowsSyncJob();
            //job2.Test();

            //var job3 = new GeniusSyncJob();
            //job3.Test();

            //var job4 = new FileSyncJob();
            //job4.Test();

            //var job5 = new FileToIppSyncJob();
            //job5.Test();

            //var job = new ZcxFileSyncJob();
            //job.Test();

            //var job7 = new BankFinFileSyncJob();
            //job7.Test();

            //var job8 = new CfpDiscFileSyncJob();
            //job8.Test();

            //var job9 = new FinPrdFileSyncJob();
            //job9.Test();

            //var job0 = new FileSyncJob();
            //job0.Test();

            //var job11 = new SyncCneZCX();
            //job11.Test();
            
        }
    }
}
