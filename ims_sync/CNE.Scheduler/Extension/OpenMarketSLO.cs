using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNE.Scheduler.Extension
{
    public class OpenMarketSLO : OpenMarketBase
    {
        protected override bool IgnoreSql(OpenMarketOperation data, ref string ignoreMessage)
        {
            if (!data._ric.Contains("SLO")) return true;
            var issueVolume = float.Parse(data._values[data.getColIdx("IssueVolume")]);
            var yield = float.Parse(data._values[data.getColIdx("Yield")]);
            if (issueVolume != 0 || yield != 0) return false;
            ignoreMessage = " <span style=\"background:yellow;\"> issueVolume = 0 && yield = 0</span>";
            return true;
        }
    }
}
