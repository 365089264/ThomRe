using System.ServiceProcess;
using CNE.Scheduler.Jobs;

namespace CNE.Scheduler
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

            var job1 = new Genius2SSJob();
            job1.Test();

            //var job = new ZCX2SSJob();
            //job.Test();

            //var job2 = new Email2SSJob();
            //job2.Test();

            //var job3 = new ZCXRR2SSAndFileDBJob();
            //job3.Test();
            //var job4 = new BOCCrawler2PDFJob();
            //job4.Test();
            //OpenMarketData open = new OpenMarketData();
            //open.Test();

            //var ftpDownloadJob = new FtpTimingDownLoad();
            //ftpDownloadJob.Test();

            //var job5 = new DM12DM2Job();
            //job5.Test();

            //var job6 = new MarketDataSync();
            //job6.Test();
        }
    }
}
