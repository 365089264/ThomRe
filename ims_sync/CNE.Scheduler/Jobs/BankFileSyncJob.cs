using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNE.Scheduler.Jobs
{
    public class BankFileSyncJob : CmaJobBase
    {
        public override string JobType
        {
            get { return "BankFileSyncJob"; }
        }

        protected override void ExecuteInternal(Quartz.JobExecutionContext context)
        {
            throw new NotImplementedException();
        }

    }
}
