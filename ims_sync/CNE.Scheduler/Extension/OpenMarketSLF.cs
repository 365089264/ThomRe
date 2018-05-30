using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNE.Scheduler.Extension
{
    public class OpenMarketSLF : OpenMarketBase
    {
        protected override bool IgnoreSql(OpenMarketOperation data, ref string ignoreMessage)
        {
            return false;
        }
    }
}
