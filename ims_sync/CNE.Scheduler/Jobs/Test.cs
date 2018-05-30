using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNE.Scheduler.Jobs
{
   public  class Test:CmaJobBase
    {
        public override string JobType
        {
            get { return "test"; }
        }

        protected override void ExecuteInternal(Quartz.JobExecutionContext context)
        {
          System.IO.File.AppendAllText(@"C:\David\test.txt", DateTime.Now.ToString() + "\r\n");
        }
    }
}
